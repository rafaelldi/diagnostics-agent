﻿namespace DiagnosticsAgent.Gc;

internal readonly record struct ValueGcEvent(
    int Number,
    string Generation,
    string Reason,
    double PauseDuration,
    double Peak,
    double After,
    double Ratio,
    double Promoted,
    double Allocated,
    double AllocationRate,
    double SizeGen0,
    double FragmentationGen0,
    double SurvivalGen0,
    double BudgetGen0,
    double SizeGen1,
    double FragmentationGen1,
    double SurvivalGen1,
    double BudgetGen1,
    double SizeGen2,
    double FragmentationGen2,
    double SurvivalGen2,
    double BudgetGen2,
    double SizeLoh,
    double FragmentationLoh,
    double SurvivalLoh,
    double BudgetLoh,
    int PinnedObjects
);