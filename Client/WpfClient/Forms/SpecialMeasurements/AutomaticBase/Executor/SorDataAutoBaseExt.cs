using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    // KeyEvents.KeyEvents[i].MarkerLocations[] (массив положений 5 маркеров)
    // 0 - левый конец левого плеча
    // 1 - правый конец левого плеча
    // 2 - левый конец правого плеча
    // 3 - правый конец правого плеча
    // 4 - пик события (максимальный уровень), по которому рассчитывается отражение


    /// <summary>
    /// В RTU4000 не реализован механизм "воротиков" (т.е. участка на котором ищутся новые события)
    /// Новые события ищутся на левом плече каждого существующего.
    /// В автоматической базовой у нас только первое и последнее событие,
    /// растягиваем левое плечо последнего события почти на всю трассу,
    /// для этого берем начало правого плеча 0го события и вписываем его в начало левого плеча последнего события.
    /// </summary>
    public static class SorDataAutoBaseExt
    {
        public static OtdrDataKnownBlocks EnhanceLeftShoulderOfLastKeyEvent(this OtdrDataKnownBlocks sorData)
        {
            var keyEvents = sorData.KeyEvents.KeyEvents;
            keyEvents[1].MarkerLocations[0] = keyEvents[0].MarkerLocations[2];

            return sorData;
        }
    }
}
