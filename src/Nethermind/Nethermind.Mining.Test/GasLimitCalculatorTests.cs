// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using FluentAssertions;
using Nethermind.Config;
using Nethermind.Consensus;
using Nethermind.Core;
using Nethermind.Core.Test.Builders;
using Nethermind.Specs;
using NUnit.Framework;

namespace Nethermind.Mining.Test
{
    [TestFixture]
    public class GasLimitCalculatorTests
    {
        [TestCase(1_000_000UL, 2_000_000, 1_000_975UL)]
        [TestCase(1_999_999UL, 2_000_000, 2_000_000UL)]
        [TestCase(2_000_000UL, 2_000_000, 2_000_000UL)]
        [TestCase(2_000_001UL, 2_000_000, 2_000_000UL)]
        [TestCase(3_000_000UL, 2_000_000, 2_997_072UL)]
        public void Test(ulong current, long target, ulong expected)
        {
            BlocksConfig blocksConfig = new();
            blocksConfig.TargetBlockGasLimit = target;

            TargetAdjustedGasLimitCalculator targetAdjustedGasLimitCalculator = new(
                MainnetSpecProvider.Instance, blocksConfig);

            BlockHeader header = Build.A.BlockHeader.WithGasLimit(current).TestObject;
            targetAdjustedGasLimitCalculator.GetGasLimit(header).Should().Be(expected);
        }
    }
}
