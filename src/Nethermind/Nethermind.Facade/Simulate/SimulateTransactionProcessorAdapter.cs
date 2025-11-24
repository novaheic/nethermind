// SPDX-FileCopyrightText: 2025 Demerzel Solutions Limited
// SPDX-License-Identifier: LGPL-3.0-only

using System;
using Nethermind.Core;
using Nethermind.Crypto;
using Nethermind.Evm;
using Nethermind.Evm.Tracing;
using Nethermind.Evm.TransactionProcessing;

namespace Nethermind.Facade.Simulate;

public class SimulateTransactionProcessorAdapter(ITransactionProcessor transactionProcessor, SimulateRequestState simulateRequestState) : ITransactionProcessorAdapter
{
    private int _currentTxIndex = 0;
    public TransactionResult Execute(Transaction transaction, ITxTracer txTracer)
    {
        // The gas limit per tx go down as the block is processed.
        if (!simulateRequestState.TxsWithExplicitGas[_currentTxIndex])
        {
            long autoGasLimit = Math.Min(simulateRequestState.BlockGasLeft, simulateRequestState.TotalGasLeft);
            autoGasLimit = Math.Max(autoGasLimit, 0);
            transaction.GasLimit = ToUnsignedGas(autoGasLimit, "auto gas limit");
        }

        long txGasLimit = ToSignedGas(transaction.GasLimit, "transaction gas limit");
        if (simulateRequestState.TotalGasLeft < txGasLimit)
        {
            long capped = Math.Max(simulateRequestState.TotalGasLeft, 0);
            transaction.GasLimit = ToUnsignedGas(capped, "available total gas");
        }

        transaction.Hash = transaction.CalculateHash();

        TransactionResult result = simulateRequestState.Validate ? transactionProcessor.Execute(transaction, txTracer) : transactionProcessor.Trace(transaction, txTracer);

        // Keep track of gas left
        long spentGas = ToSignedGas(transaction.SpentGas, "transaction spent gas");
        simulateRequestState.TotalGasLeft -= spentGas;
        simulateRequestState.BlockGasLeft -= spentGas;

        _currentTxIndex++;
        return result;
    }

    public void SetBlockExecutionContext(in BlockExecutionContext blockExecutionContext)
    {
        _currentTxIndex = 0;
        transactionProcessor.SetBlockExecutionContext(in blockExecutionContext);
    }

    private static long ToSignedGas(ulong gas, string source)
    {
        if (gas > long.MaxValue)
        {
            throw new OverflowException($"{source} ({gas}) exceeds supported range.");
        }

        return (long)gas;
    }

    private static ulong ToUnsignedGas(long gas, string source)
    {
        if (gas < 0)
        {
            throw new InvalidOperationException($"{source} ({gas}) cannot be negative.");
        }

        return (ulong)gas;
    }
}
