// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only


using FluentAssertions;
using Nethermind.Config;
using Nethermind.Core;
using Nethermind.Core.Specs;
using Nethermind.Core.Test.Builders;
using Nethermind.Specs;
using Nethermind.Specs.Forks;
using Nethermind.Specs.Test;
using NUnit.Framework;

namespace Nethermind.Consensus.Test
{
    public class TargetAdjustedGasLimitCalculatorTests
    {
        [Test]
        public void Is_bump_on_1559_eip_block()
        {
            int londonBlock = 5;
            ulong gasLimit = 1000000000000000000;
            OverridableReleaseSpec spec = new(London.Instance)
            {
                Eip1559TransitionBlock = londonBlock
            };
            TestSpecProvider specProvider = new(spec);
            TargetAdjustedGasLimitCalculator targetedAdjustedGasLimitCalculator = new(specProvider, new BlocksConfig());
            BlockHeader header = Build.A.BlockHeader.WithNumber(londonBlock - 1).WithGasLimit(gasLimit).TestObject;
            ulong actualValue = targetedAdjustedGasLimitCalculator.GetGasLimit(header);
            Assert.That(actualValue, Is.EqualTo(gasLimit * (ulong)Eip1559Constants.DefaultElasticityMultiplier));
        }

        [TestCase(30_000_000UL, 100_000_000L, 30_029_295UL)]
        public void Is_calculating_correct_gasLimit(ulong currentGasLimit, long targetGasLimit, ulong expectedGasLimit)
        {
            int blockNumber = 20_000_000;
            ulong gasLimit = currentGasLimit;
            TestSpecProvider specProvider = new(Prague.Instance);
            TargetAdjustedGasLimitCalculator targetedAdjustedGasLimitCalculator = new(specProvider,
                new BlocksConfig()
                {
                    TargetBlockGasLimit = targetGasLimit
                });
            BlockHeader header = Build.A.BlockHeader.WithNumber(blockNumber - 1).WithGasLimit(gasLimit).TestObject;
            ulong actualValue = targetedAdjustedGasLimitCalculator.GetGasLimit(header);
            actualValue.Should().Be(expectedGasLimit);
        }

        [Test]
        public void Doesnt_go_below_minimum()
        {
            int londonBlock = 5;
            ulong gasLimit = 5000;
            TestSpecProvider specProvider = new(London.Instance);
            TargetAdjustedGasLimitCalculator targetedAdjustedGasLimitCalculator = new(specProvider, new BlocksConfig() { TargetBlockGasLimit = 1 });
            BlockHeader header = Build.A.BlockHeader.WithNumber(londonBlock - 1).WithGasLimit(gasLimit).TestObject;
            ulong actualValue = targetedAdjustedGasLimitCalculator.GetGasLimit(header);
            ulong minGasLimit = (ulong)specProvider.GetSpec(new ForkActivation(5)).MinGasLimit;
            Assert.That(actualValue, Is.EqualTo(minGasLimit));
        }
    }
}
