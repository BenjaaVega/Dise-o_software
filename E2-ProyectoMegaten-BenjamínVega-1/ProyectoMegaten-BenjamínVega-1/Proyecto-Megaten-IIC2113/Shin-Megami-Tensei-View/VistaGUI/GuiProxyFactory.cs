using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Shin_Megami_Tensei_View.VistaGUI;

internal sealed class GuiProxyFactory
{
    private readonly GuiLibrary _library;
    private readonly ModuleBuilder _module;
    private readonly Type _unitProxyType;
    private readonly Type _playerProxyType;
    private readonly Type _stateProxyType;

    public GuiProxyFactory(GuiLibrary library)
    {
        _library = library;
        var assemblyName = new AssemblyName("SMTGuiDynamicProxies");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        _module = assemblyBuilder.DefineDynamicModule(assemblyName.Name!);

        _unitProxyType = BuildUnitProxy();
        _playerProxyType = BuildPlayerProxy();
        _stateProxyType = BuildStateProxy();
    }

    public object CreateState(GuiStateModel model)
    {
        var player1 = CreatePlayer(model.Player1);
        var player2 = CreatePlayer(model.Player2);
        var options = new List<string>(model.Options);
        var order = new List<string>(model.Order);

        return Activator.CreateInstance(
                   _stateProxyType,
                   new object[]
                   {
                       player1,
                       player2,
                       options,
                       model.Turns,
                       model.BlinkingTurns,
                       order
                   })
               ?? throw new InvalidOperationException("No se pudo crear el proxy de IState.");
    }

    private object CreatePlayer(GuiPlayerState state)
    {
        var boardArray = Array.CreateInstance(_library.UnitType, state.Board.Length);
        for (int i = 0; i < state.Board.Length; i++)
        {
            var unit = state.Board[i];
            boardArray.SetValue(unit is null ? null : CreateUnit(unit), i);
        }

        var listType = typeof(List<>).MakeGenericType(_library.UnitType);
        var reserveList = (IList)Activator.CreateInstance(listType)!;
        foreach (var unit in state.Reserve)
        {
            reserveList.Add(CreateUnit(unit));
        }

        return Activator.CreateInstance(
                   _playerProxyType,
                   new object[]
                   {
                       boardArray,
                       reserveList
                   })
               ?? throw new InvalidOperationException("No se pudo crear el proxy de IPlayer.");
    }

    private object CreateUnit(GuiUnitState unit)
    {
        return Activator.CreateInstance(
                   _unitProxyType,
                   new object[]
                   {
                       unit.Name,
                       unit.Hp,
                       unit.Mp,
                       unit.MaxHp,
                       unit.MaxMp
                   })
               ?? throw new InvalidOperationException("No se pudo crear el proxy de IUnit.");
    }

    private Type BuildUnitProxy()
    {
        var builder = _module.DefineType(
            "SMTGuiUnitProxy",
            TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed);

        builder.AddInterfaceImplementation(_library.UnitType);

        var stringType = typeof(string);
        var intType = typeof(int);

        var nameField = builder.DefineField("_name", stringType, FieldAttributes.Private);
        var hpField = builder.DefineField("_hp", intType, FieldAttributes.Private);
        var mpField = builder.DefineField("_mp", intType, FieldAttributes.Private);
        var maxHpField = builder.DefineField("_maxHp", intType, FieldAttributes.Private);
        var maxMpField = builder.DefineField("_maxMp", intType, FieldAttributes.Private);

        var ctor = builder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            new[] { stringType, intType, intType, intType, intType });

        var il = ctor.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Stfld, nameField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Stfld, hpField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_3);
        il.Emit(OpCodes.Stfld, mpField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_S, 4);
        il.Emit(OpCodes.Stfld, maxHpField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_S, 5);
        il.Emit(OpCodes.Stfld, maxMpField);
        il.Emit(OpCodes.Ret);

        ImplementGetter(builder, nameField, "Name", stringType);
        ImplementGetter(builder, hpField, "HP", intType);
        ImplementGetter(builder, mpField, "MP", intType);
        ImplementGetter(builder, maxHpField, "MaxHP", intType);
        ImplementGetter(builder, maxMpField, "MaxMP", intType);

        return builder.CreateTypeInfo()!.AsType();
    }

    private Type BuildPlayerProxy()
    {
        var builder = _module.DefineType(
            "SMTGuiPlayerProxy",
            TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed);

        builder.AddInterfaceImplementation(_library.PlayerType);

        var boardType = _library.UnitType.MakeArrayType();
        var reserveType = typeof(IEnumerable<>).MakeGenericType(_library.UnitType);

        var boardField = builder.DefineField("_board", boardType, FieldAttributes.Private);
        var reserveField = builder.DefineField("_reserve", reserveType, FieldAttributes.Private);

        var ctor = builder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            new[] { boardType, reserveType });

        var il = ctor.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Stfld, boardField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Stfld, reserveField);
        il.Emit(OpCodes.Ret);

        ImplementGetter(builder, boardField, "UnitsInBoard", boardType);
        ImplementGetter(builder, reserveField, "UnitsInReserve", reserveType);

        return builder.CreateTypeInfo()!.AsType();
    }

    private Type BuildStateProxy()
    {
        var builder = _module.DefineType(
            "SMTGuiStateProxy",
            TypeAttributes.Class | TypeAttributes.NotPublic | TypeAttributes.Sealed);

        builder.AddInterfaceImplementation(_library.StateType);

        var optionsType = typeof(IEnumerable<string>);
        var orderType = typeof(IEnumerable<string>);
        var intType = typeof(int);

        var player1Field = builder.DefineField("_player1", _library.PlayerType, FieldAttributes.Private);
        var player2Field = builder.DefineField("_player2", _library.PlayerType, FieldAttributes.Private);
        var optionsField = builder.DefineField("_options", optionsType, FieldAttributes.Private);
        var turnsField = builder.DefineField("_turns", intType, FieldAttributes.Private);
        var blinkingField = builder.DefineField("_blinking", intType, FieldAttributes.Private);
        var orderField = builder.DefineField("_order", orderType, FieldAttributes.Private);

        var ctor = builder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            new[]
            {
                _library.PlayerType,
                _library.PlayerType,
                optionsType,
                intType,
                intType,
                orderType
            });

        var il = ctor.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Stfld, player1Field);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Stfld, player2Field);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_3);
        il.Emit(OpCodes.Stfld, optionsField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_S, 4);
        il.Emit(OpCodes.Stfld, turnsField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_S, 5);
        il.Emit(OpCodes.Stfld, blinkingField);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_S, 6);
        il.Emit(OpCodes.Stfld, orderField);
        il.Emit(OpCodes.Ret);

        ImplementGetter(builder, player1Field, "Player1", _library.PlayerType);
        ImplementGetter(builder, player2Field, "Player2", _library.PlayerType);
        ImplementGetter(builder, optionsField, "Options", optionsType);
        ImplementGetter(builder, turnsField, "Turns", intType);
        ImplementGetter(builder, blinkingField, "BlinkingTurns", intType);
        ImplementGetter(builder, orderField, "Order", orderType);

        return builder.CreateTypeInfo()!.AsType();
    }

    private void ImplementGetter(TypeBuilder builder, FieldInfo field, string propertyName, Type propertyType)
    {
        var property = builder.DefineProperty(propertyName, PropertyAttributes.None, propertyType, Type.EmptyTypes);
        var method = builder.DefineMethod(
            "get_" + propertyName,
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
            propertyType,
            Type.EmptyTypes);

        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldfld, field);
        il.Emit(OpCodes.Ret);

        property.SetGetMethod(method);
        builder.DefineMethodOverride(method, ResolveGetter(propertyName, propertyType));
    }

    private MethodInfo ResolveGetter(string propertyName, Type propertyType)
    {
        var targetType = propertyName switch
        {
            "Name" or "HP" or "MP" or "MaxHP" or "MaxMP" => _library.UnitType,
            "UnitsInBoard" or "UnitsInReserve" => _library.PlayerType,
            _ => _library.StateType
        };

        return targetType.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!.GetMethod!;
    }
}
