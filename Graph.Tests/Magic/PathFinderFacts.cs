using System;
using System.Collections.Generic;
using FluentAssertions;
using Iit.Fibertest.Graph.Magic;
using Xunit;

namespace Graph.Tests.Magic
{
    public sealed class PathFinderFacts
    {
        private static readonly Dictionary<int, HashSet<int>> GraphSample
            = new Dictionary<int, HashSet<int>>();

        static PathFinderFacts()
        {
            GraphSample.Add(1, new HashSet<int>());
            E(2, 3);

            E(4, 5);
            E(4, 6);
            E(5, 6);
            E(5, 7);
            E(6, 7);

            E(8, 9);
            E(9, 10);
            E(8, 11);
            E(11, 12);

        }

        private static HashSet<int> From(int key)
        {
            HashSet<int> value;
            if (!GraphSample.TryGetValue(key, out value))
                GraphSample[key] = value = new HashSet<int>();
            return value;
        }

        private static void Set(HashSet<int> set, int item)
        {
            if (!set.Add(item))
                throw new InvalidOperationException();
        }
        private static void E(int src, int dest)
        {
            Set(From(src), dest);
            Set(From(dest), src);
        }

        [Fact]
        public void BadFindPathTo_Finds_Long_Path()
        {
            4.BadFindPathTo(7, x => GraphSample[x])
                .Should().Equal(4, 5, 6, 7); // it could be 4, 5, 7
        }
        [Fact]
        public void Good_FindPathTo_Finds_Shorter_Path()
        {
            4.FindPathTo(7, x => GraphSample[x])
                .Should().Equal(4, 5, 7);
        }
        [Fact]
        public void Good_FindPathTo_No_Path()
        {
            1.FindPathTo(7, x => GraphSample[x])
                .Should().BeEmpty();
        }
        [Fact]
        public void Good_FindPathTo_Start_Equals_End()
        {
            5.FindPathTo(5, x => GraphSample[x]).Should().Equal(5);
        }
        [Fact]
        public void Good_FindPathTo_Short_Path()
        {
            2.FindPathTo(3, x => GraphSample[x]).Should().Equal(2, 3);
        }
        [Fact]
        public void Internal_Discovery_Stops_When_TheEnd_IsFound()
        {
            8.FindPathTo(9, x => GraphSample[x]).Should().Equal(8, 9);
            8.FindPathTo(11, x => GraphSample[x]).Should().Equal(8, 11);
        }
    }
}