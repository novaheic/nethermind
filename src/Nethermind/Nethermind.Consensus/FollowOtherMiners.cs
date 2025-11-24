// SPDX-FileCopyrightText: 2022 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using Nethermind.Core;
using Nethermind.Core.Specs;

namespace Nethermind.Consensus
{
    public class FollowOtherMiners : IGasLimitCalculator
    {
        private readonly ISpecProvider _specProvider;

        public FollowOtherMiners(ISpecProvider specProvider)
        {
            _specProvider = specProvider;
        }

        public ulong GetGasLimit(BlockHeader parentHeader)
        {
            long parentGasLimit = ToSignedGas(parentHeader.GasLimit, "parent gas limit");
            long newBlockNumber = parentHeader.Number + 1;
            IReleaseSpec spec = _specProvider.GetSpec(parentHeader);
            long gasLimit = Eip1559GasLimitAdjuster.AdjustGasLimit(spec, parentGasLimit, newBlockNumber);
            return (ulong)gasLimit;
        }

        private static long ToSignedGas(ulong gasLimit, string source)
        {
            if (gasLimit > long.MaxValue)
            {
                throw new OverflowException($"{source} ({gasLimit}) exceeds supported range.");
            }

            return (long)gasLimit;
        }
    }
}
