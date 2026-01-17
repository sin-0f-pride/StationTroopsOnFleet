using System.Collections.Generic;

using StationTroopsWithFleet.Station;

namespace StationTroopsWithFleet
{
    internal class SaveableTypeDefiner : TaleWorlds.SaveSystem.SaveableTypeDefiner
    {
        public SaveableTypeDefiner() : base((0x1f3dd5 << 8) | 123) { }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(StationedTroops), 1);
            AddClassDefinition(typeof(StationedTroopsManager), 2);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<StationedTroops>));
        }
    }
}
