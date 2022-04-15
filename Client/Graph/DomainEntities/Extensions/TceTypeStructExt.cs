using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class TceTypeStructExt
    {
        public static TceS CreateTce(this TceTypeStruct tceTypeStruct)
        {
            var tce = new TceS() { TceTypeStruct = tceTypeStruct, SlotCount = tceTypeStruct.SlotCount };
            foreach (var slot in tceTypeStruct.SlotPositions)
            {
                tce.Slots.Add(new TceSlot() { Position = slot, GponInterfaceCount = 0 });
            }
            return tce;
        }

        public static IEnumerable<TceTypeStruct> Generate()
        {
            // ===== HUAWEI =================================

            yield return new TceTypeStruct()
            {
                Id = 100,
                IsVisible = true,
                Maker = TceMaker.Huawei,
                Title = @"MA5608T",
                SoftwareVersion = @"",
                Code = @"Huawei_MA5608T_v1", // investigated 2021
                SlotCount = 2,
                SlotPositions = new[] { 1, 2 },
                Comment = "",
            };

            yield return new TceTypeStruct()
            {
                Id = 101,
                IsVisible = true,
                Maker = TceMaker.Huawei,
                Title = @"MA5600T (19″)",
                SoftwareVersion = @"v800R018C10, SPH212 HP2112",
                Code = @"Huawei_MA5600T_v1", // investigated 2022-Apr-12
                SlotCount = 14,
                SlotPositions = new[] { 1, 2, 3, 4, 5, 6, 9, 10, 11, 12, 13, 14, 15, 16 },
                Comment = "",
            };

            yield return new TceTypeStruct()
            {
                Id = 102,
                IsVisible = false,
                Maker = TceMaker.Huawei,
                Title = @"MA5600T (19″)",
                SoftwareVersion = @"",
                Code = @"Huawei_MA5600T_v2", // has not investigated yet
                SlotCount = 14,
                SlotPositions = new[] { 1, 2, 3, 4, 5, 6, 9, 10, 11, 12, 13, 14, 15, 16 },
                Comment = "",
            };

            yield return new TceTypeStruct()
            {
                Id = 103,
                IsVisible = false,
                Maker = TceMaker.Huawei,
                Title = @"MA5600T (19″)",
                SoftwareVersion = @"",
                Code = @"Huawei_MA5600T_v3", // has not investigated yet
                SlotCount = 14,
                SlotPositions = new[] { 1, 2, 3, 4, 5, 6, 9, 10, 11, 12, 13, 14, 15, 16 },
                Comment = "",
            };

            // ===== ZTE ====================================

            yield return new TceTypeStruct()
            {
                Id = 200,
                IsVisible = true,
                Maker = TceMaker.ZTE,
                Title = @"C320",
                SoftwareVersion = @"",
                Code = @"ZTE_C320_v1", // investigated 2021
                SlotCount = 2,
                SlotPositions = new[] { 1, 2 },
                Comment = "",
            };

            yield return new TceTypeStruct()
            {
                Id = 201,
                IsVisible = true,
                Maker = TceMaker.ZTE,
                Title = @"C300 (19″)",
                SoftwareVersion = @"V1.2.5P3",
                Code = @"ZTE_C300_v1", // investigated 2022-Apr-12
                SlotCount = 14,
                SlotPositions = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 12, 13, 14, 15, 16, 17 },
                Comment = "",
            };

            yield return new TceTypeStruct()
            {
                Id = 202,
                IsVisible = true,
                Maker = TceMaker.ZTE,
                Title = @"C300M (19″)",
                SoftwareVersion = @"V4.0.2P2",
                Code = @"ZTE_C300M_v1", // investigated 2022-Apr-12
                SlotCount = 14,
                SlotPositions = new[] { 2, 3, 4, 5, 6, 7, 8, 9, 12, 13, 14, 15, 16, 17 },
                Comment = "",
            };
        }
    }
}
