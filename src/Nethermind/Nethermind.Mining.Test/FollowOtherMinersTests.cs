// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using FluentAssertions;
using Nethermind.Consensus;
using Nethermind.Core;
using Nethermind.Core.Test.Builders;
using Nethermind.Specs;
using Nethermind.Specs.Forks;
using Nethermind.Specs.Test;
using NUnit.Framework;

namespace Nethermind.Mining.Test
{
    [TestFixture]
    public class FollowOtherMinersTests
    {
        [TestCase(1_000_000UL, 1_000_000UL)]
        [TestCase(1_999_999UL, 1_999_999UL)]
        [TestCase(2_000_000UL, 2_000_000UL)]
        [TestCase(2_000_001UL, 2_000_001UL)]
        [TestCase(3_000_000UL, 3_000_000UL)]
        public void Test(ulong current, ulong expected)
        {
            BlockHeader header = Build.A.BlockHeader.WithGasLimit(current).TestObject;
            FollowOtherMiners followOtherMiners = new(MainnetSpecProvider.Instance);
            followOtherMiners.GetGasLimit(header).Should().Be(expected);
        }

        [TestCase(1_000_000UL, 2_000_000UL)]
        [TestCase(2_000_000UL, 4_000_000UL)]
        [TestCase(2_000_001UL, 4_000_002UL)]
        [TestCase(3_000_000UL, 6_000_000UL)]
        public void FollowOtherMines_on_1559_fork_block(ulong current, ulong expected)
        {
            int forkNumber = 5;
            OverridableReleaseSpec spec = new(London.Instance)
            {
                Eip1559TransitionBlock = forkNumber
            };
            TestSpecProvider specProvider = new(spec);
            BlockHeader header = Build.A.BlockHeader.WithGasLimit(current).WithNumber(forkNumber - 1).TestObject;
            FollowOtherMiners followOtherMiners = new(specProvider);
            followOtherMiners.GetGasLimit(header).Should().Be(expected);
        }
    }
}
