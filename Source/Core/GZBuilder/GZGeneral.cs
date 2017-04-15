#region ================== Namespaces

using CodeImp.DoomBuilder.Config;
using CodeImp.DoomBuilder.GZBuilder.Data;
using CodeImp.DoomBuilder.Map;
using CodeImp.DoomBuilder.ZDoom;
using System;

#endregion

namespace CodeImp.DoomBuilder.GZBuilder
{
	//mxd. should get rid of this class one day...
	public static class GZGeneral
    {
		#region ================== Properties

		//gzdoom light types
		private static readonly int[] gzLights = {
            /* normal lights */ 9800, 9801, 9802, 9803, 9804,
            /* additive lights */ 9810, 9811, 9812, 9813, 9814,
            /* negative lights */ 9820, 9821, 9822, 9823, 9824,
            /* attenuated lights */ 9830, 9831, 9832, 9833, 9834,
            /* vavoom lights */ 1502, 1503};
		public static int[] GZ_LIGHTS { get { return gzLights; } }
		private static readonly int[] gzLightTypes = { 5, 10, 15, 20 }; //these are actually offsets in gz_lights
		public static int[] GZ_LIGHT_TYPES { get { return gzLightTypes; } }
		private static readonly DynamicLightType[] gzAnimatedLightTypes = { DynamicLightType.FLICKER, DynamicLightType.RANDOM, DynamicLightType.PULSE };
		public static DynamicLightType[] GZ_ANIMATED_LIGHT_TYPES { get { return gzAnimatedLightTypes; } }

		//asc script action specials
		private static readonly int[] acsSpecials = { 80, 81, 82, 83, 84, 85, 226 };
		public static int[] ACS_SPECIALS { get { return acsSpecials; } }

        // [ZZ] this is for proper inheritance of lights.
        //      technically this can be found by parsing gzdoom.pk3/mapinfo/common.txt, but I wouldn't do that without a good reason for now.
        private static readonly string[] gzLightClasses =
        {
            /* normal lights */ "pointlight", "pointlightpulse", "pointlightflicker", "sectorpointlight", "pointlightflickerrandom",
            /* additive lights */ "pointlightadditive", "pointlightpulseadditive", "pointlightflickeradditive", "sectorpointlightadditive", "pointlightflickerrandomadditive",
            /* subtractive lights */ "pointlightsubtractive", "pointlightpulsesubtractive", "pointlightflickersubtractive", "sectorpointlightsubtractive", "pointlightflickerrandomsubtractive",
            /* attenuated lights */ "pointlightattenuated", "pointlightpulseattenuated", "pointlightflickerattenuated", "sectorpointlightattenuated", "pointlightflickerrandomattenuated",
            /* vavoom lights */ "vavoomlightwhite", "vavoomlightcolor"
        };

        public static int GetGZLightTypeByClass(ActorStructure actor)
        {
            int idx = -1;
            ActorStructure p = actor;
            while (p != null)
            {
                idx = Array.IndexOf(gzLightClasses, p.ClassName.ToLowerInvariant());
                if (idx != -1)
                {
                    // found dynamic light type. alter it by actor flags.
                    // +MISSILEMORE makes it additive.
                    // +MISSILEEVENMORE makes it subtractive.
                    // +INCOMBAT makes it attenuated.
                    int light = gzLights[idx];
                    if (idx < GZ_LIGHT_TYPES[3])
                    {
                        int baseType = light % 10;
                        int dispType = light - baseType;
                        if (actor.GetFlagValue("MISSILEMORE", false))
                            dispType = 9810;
                        else if (actor.GetFlagValue("MISSILEEVENMORE", false))
                            dispType = 9820;
                        else if (actor.GetFlagValue("INCOMBAT", false))
                            dispType = 9830;
                        return dispType + baseType;
                    }
                    else return light;
                }

                p = p.BaseClass;
            }

            return 0;
        }

        public static int GetGZLightTypeByThing(Thing t)
        {
            int type = Array.IndexOf(gzLights, t.DynamicLightType);
            if (type >= 0)
                return type;

            return -1;
        }

        // this is here so that I can see all dirty patches by listing references to this method.
        public static void AssertNotNull(object o, string whatisit)
        {
            if (o == null)
            {
                throw new NullReferenceException(whatisit + " is null");
            }
        }

        #endregion

    }
}