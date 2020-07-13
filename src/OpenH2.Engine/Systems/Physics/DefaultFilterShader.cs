using PhysX;

namespace OpenH2.Engine.Systems.Physics
{
    public enum OpenH2FilterData : uint
    {
        PlayerCharacter = 1 << 0
    }

    public class DefaultFilterShader : SimulationFilterShader
    {
        private static FilterResult DefaultResult = new FilterResult()
        {
            PairFlags = PairFlag.ContactDefault,
            FilterFlag = FilterFlag.Default
        };

        private static FilterResult PlayerCharacterResult = new FilterResult()
        {
            PairFlags = PairFlag.ModifyContacts | PairFlag.ContactDefault | PairFlag.TriggerDefault,
            FilterFlag = FilterFlag.Default
        };

        public override FilterResult Filter(int attributes0, FilterData filterData0, int attributes1, FilterData filterData1)
        {
            if(filterData0.Word0 == 0 && filterData1.Word0 == 0)
            {
                return DefaultResult;
            }

            if(IsPlayerCharacter(filterData0) || IsPlayerCharacter(filterData1))
            {
                return PlayerCharacterResult;
            }

            return DefaultResult;
        }

        private bool IsPlayerCharacter(FilterData fd) => (((OpenH2FilterData)fd.Word0) & OpenH2FilterData.PlayerCharacter) == OpenH2FilterData.PlayerCharacter;
    }
}
