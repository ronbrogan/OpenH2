using PhysX;

namespace OpenH2.Engine.Systems.Physics
{
    public enum OpenH2FilterData : uint
    {
        PlayerCharacter = 1 << 0,
        AiActor         = 1 << 1,
        TriggerVolume   = 1 << 2,
        NoClip          = 1 << 3,

        TriggerSubject = PlayerCharacter | AiActor
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
            PairFlags = PairFlag.ModifyContacts | PairFlag.ContactDefault,
            FilterFlag = FilterFlag.Default
        };

        private static FilterResult TriggerVolumeResult = new FilterResult()
        {
            PairFlags = PairFlag.TriggerDefault,
            FilterFlag = FilterFlag.Default
        };

        public override FilterResult Filter(int attributes0, FilterData filterData0, int attributes1, FilterData filterData1)
        {
            if(filterData0.Word0 == 0 && filterData1.Word0 == 0)
            {
                return DefaultResult;
            }

            var result = DefaultResult;
            result.FilterFlag = FilterFlag.Default;
            
            if(IsPlayerCharacter(filterData0) || IsPlayerCharacter(filterData1))
            {
                result.PairFlags |= PlayerCharacterResult.PairFlags;
            }

            if ((IsTriggerSubject(filterData0) || IsTriggerSubject(filterData1)) &&
                (IsTriggerVolume(filterData0) || IsTriggerVolume(filterData1)))
            {
                result.PairFlags |= TriggerVolumeResult.PairFlags;
            }

            if(IsNoClip(filterData0) || IsNoClip(filterData1))
            {
                // TODO: surface PairFlag.SolveContact in package
                result.PairFlags = (result.PairFlags) & (PairFlag)(~1);
            }

            return result;
        }

        private bool IsPlayerCharacter(FilterData fd) => (((OpenH2FilterData)fd.Word0) & OpenH2FilterData.PlayerCharacter) == OpenH2FilterData.PlayerCharacter;
        private bool IsNoClip(FilterData fd) => (((OpenH2FilterData)fd.Word0) & OpenH2FilterData.NoClip) == OpenH2FilterData.NoClip;

        private bool IsTriggerVolume(FilterData fd) => (((OpenH2FilterData)fd.Word0) & OpenH2FilterData.TriggerVolume) == OpenH2FilterData.TriggerVolume;
        private bool IsTriggerSubject(FilterData fd) => (((OpenH2FilterData)fd.Word0) & OpenH2FilterData.TriggerSubject) != 0;
    }
}
