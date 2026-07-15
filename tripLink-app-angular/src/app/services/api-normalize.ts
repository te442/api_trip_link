import { OptimizationProgress, OptimizationStepTrace, OptimizeResult, ScoreTableCellTrace, ScoreTableStats } from '../models/models';

export function normalizeScoreTableCell(c: Record<string, unknown> | null | undefined): ScoreTableCellTrace {
  const row = c ?? {};
  return {
    seq: Number(row['seq'] ?? row['Seq'] ?? 0) || undefined,
    i: Number(row['i'] ?? row['I'] ?? 0),
    j: Number(row['j'] ?? row['J'] ?? 0),
    h: Number(row['h'] ?? row['H'] ?? 0),
    fromLabel: String(row['fromLabel'] ?? row['FromLabel'] ?? ''),
    toLabel: String(row['toLabel'] ?? row['ToLabel'] ?? ''),
    departureTime: String(row['departureTime'] ?? row['DepartureTime'] ?? ''),
    apiKind: String(row['apiKind'] ?? row['ApiKind'] ?? ''),
    fromCache: Boolean(row['fromCache'] ?? row['FromCache'] ?? false),
    isValid: Boolean(row['isValid'] ?? row['IsValid'] ?? false),
    transitionScore: Number(row['transitionScore'] ?? row['TransitionScore'] ?? 0),
    busTransitHours: Number(row['busTransitHours'] ?? row['BusTransitHours'] ?? 0),
    walkingHours: Number(row['walkingHours'] ?? row['WalkingHours'] ?? 0),
    transitEfficiency: Number(row['transitEfficiency'] ?? row['TransitEfficiency'] ?? 0),
    hasDirectBus: Boolean(row['hasDirectBus'] ?? row['HasDirectBus'] ?? false),
  };
}

export function normalizeOptimizationProgress(raw: Record<string, unknown> | null | undefined): OptimizationProgress {
  const p = raw ?? {};
  const cells = (p['scoreTableCells'] ?? p['ScoreTableCells'] ?? []) as Record<string, unknown>[];
  const steps = (p['steps'] ?? p['Steps'] ?? []) as Record<string, unknown>[];

  return {
    traceId: String(p['traceId'] ?? p['TraceId'] ?? ''),
    isComplete: Boolean(p['isComplete'] ?? p['IsComplete'] ?? false),
    hasError: Boolean(p['hasError'] ?? p['HasError'] ?? false),
    errorMessage: (p['errorMessage'] ?? p['ErrorMessage']) as string | undefined,
    scoreTableCellsBuilt: (p['scoreTableCellsBuilt'] ?? p['ScoreTableCellsBuilt']) as number | undefined,
    scoreTableCellsTotal: (p['scoreTableCellsTotal'] ?? p['ScoreTableCellsTotal']) as number | undefined,
    scoreTableHttpRequestsCompleted: (p['scoreTableHttpRequestsCompleted'] ?? p['ScoreTableHttpRequestsCompleted']) as number | undefined,
    scoreTableHttpRequestsEstimated: (p['scoreTableHttpRequestsEstimated'] ?? p['ScoreTableHttpRequestsEstimated']) as number | undefined,
    scoreTableCells: cells.map(normalizeScoreTableCell),
    steps: steps.map(s => ({
      stepNumber: Number(s['stepNumber'] ?? s['StepNumber'] ?? 0),
      stepName: String(s['stepName'] ?? s['StepName'] ?? ''),
      label: String(s['label'] ?? s['Label'] ?? ''),
      status: (s['status'] ?? s['Status'] ?? 'Pending') as OptimizationStepTrace['status'],
      detail: (s['detail'] ?? s['Detail']) as string | undefined,
      startedAt: (s['startedAt'] ?? s['StartedAt']) as string | undefined,
      finishedAt: (s['finishedAt'] ?? s['FinishedAt']) as string | undefined,
      durationMs: (s['durationMs'] ?? s['DurationMs']) as number | undefined,
    })),
  };
}

export function normalizeOptimizeResult(raw: Record<string, unknown> | null | undefined): OptimizeResult {
  const r = raw ?? {};
  const trace = (r['scoreTableCellTrace'] ?? r['ScoreTableCellTrace'] ?? []) as Record<string, unknown>[];
  const pipeline = (r['pipelineTrace'] ?? r['PipelineTrace'] ?? []) as Record<string, unknown>[];
  const stats = (r['scoreTableStats'] ?? r['ScoreTableStats']) as Record<string, unknown> | undefined;

  const scoreTableStats: ScoreTableStats | undefined = stats ? {
    nodeCount: Number(stats['nodeCount'] ?? stats['NodeCount'] ?? 0),
    minuteCount: Number(stats['minuteCount'] ?? stats['MinuteCount'] ?? stats['hourCount'] ?? stats['HourCount'] ?? 0),
    hourCount: Number(stats['hourCount'] ?? stats['HourCount'] ?? stats['minuteCount'] ?? stats['MinuteCount'] ?? 0),
    totalCells: Number(stats['totalCells'] ?? stats['TotalCells'] ?? 0),
    validCells: Number(stats['validCells'] ?? stats['ValidCells'] ?? 0),
    validRatio: Number(stats['validRatio'] ?? stats['ValidRatio'] ?? 0),
    description: String(stats['description'] ?? stats['Description'] ?? ''),
  } : undefined;

  return {
    ...(r as unknown as OptimizeResult),
    returnLeg: (r['returnLeg'] ?? r['ReturnLeg']) as OptimizeResult['returnLeg'],
    scoreTableCellTrace: trace.map(normalizeScoreTableCell),
    pipelineTrace: pipeline.map(s => ({
      stepNumber: Number(s['stepNumber'] ?? s['StepNumber'] ?? 0),
      stepName: String(s['stepName'] ?? s['StepName'] ?? ''),
      label: String(s['label'] ?? s['Label'] ?? ''),
      status: (s['status'] ?? s['Status'] ?? 'Pending') as OptimizationStepTrace['status'],
      detail: (s['detail'] ?? s['Detail']) as string | undefined,
      startedAt: (s['startedAt'] ?? s['StartedAt']) as string | undefined,
      finishedAt: (s['finishedAt'] ?? s['FinishedAt']) as string | undefined,
      durationMs: (s['durationMs'] ?? s['DurationMs']) as number | undefined,
    })),
    scoreTableStats,
  };
}
