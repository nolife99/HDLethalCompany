using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace HDLethalCompany.Tools;

internal static class Reflection
{
    public static Func<HUDManager, Dictionary<RectTransform, ScanNodeProperties>> get_scanNodes;

    public static void CompileTypedExpression()
    {
        var target = Expression.Parameter(typeof(HUDManager));
        get_scanNodes = Expression.Lambda<Func<HUDManager, Dictionary<RectTransform, ScanNodeProperties>>>(
            Expression.Field(target, target.Type.GetField("scanNodes", BindingFlags.Instance | BindingFlags.NonPublic)), true, target).Compile();
    }
}