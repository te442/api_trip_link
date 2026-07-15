// ─── Angular Models (matching API DTOs) ──────────────────────────────────────

export interface Destination {
  desId: number;
  nameDes: string;
  region: string;
  levelId?: number;
  levelType?: string;
  travelerId?: number;
  travelerType?: string;
  timeDes?: number | string;
  openingTime?: string;
  closingTime?: string;
  imageUrl?: string;
  categories?: string[];
}

export interface Trip {
  tripId: number;
  tripName: string;
  userId?: string;
  userName?: string;
  tripDate?: string;
  addressStart?: string;
  startTime?: string;
  endTime?: string;
  tripCost?: number;
  destinations?: Destination[];
}

export interface CreateTrip {
  tripName: string;
  userId?: string;
  tripDate?: string;
  addressStart?: string;
  startTime?: string;
  endTime?: string;
  tripCost?: number;
  categoryIds?: number[];
  featureIds?: number[];
  levelId?: number;
  minNumDes?: number;
  maxNumDes?: number;
  region?: string;
}

export interface User {
  userId: string;
  fullName: string;
  phone: string;
}

export interface CreateUser {
  fullName: string;
  phone: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  phone: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  userId: string;
  fullName: string;
  email: string;
}

export interface Station {
  stationNum: number;
  stationCode: string;
  stationName: string;
  area: string;
  lat?: number;
  lon?: number;
}

export interface OptimizeRequest {
  tripId: number;
  tripStartTime: string;
  tripEndTime: string;
  maxTravelTime: number;
  returnTravelTime: number;
  minTransitEfficiency?: number;
  traceId?: string;
}

export interface OptimizationStepTrace {
  stepNumber: number;
  stepName: string;
  label: string;
  status: 'Pending' | 'Running' | 'Completed' | 'Failed';
  detail?: string;
  startedAt?: string;
  finishedAt?: string;
  durationMs?: number;
}

export interface OptimizationProgress {
  traceId: string;
  isComplete: boolean;
  hasError: boolean;
  errorMessage?: string;
  steps: OptimizationStepTrace[];
  scoreTableCellsBuilt?: number;
  scoreTableCellsTotal?: number;
  scoreTableHttpRequestsCompleted?: number;
  scoreTableHttpRequestsEstimated?: number;
  scoreTableCells?: ScoreTableCellTrace[];
}

export interface ScoreTableCellTrace {
  seq?: number;
  i: number;
  j: number;
  h: number;
  fromLabel: string;
  toLabel: string;
  departureTime: string;
  apiKind?: string;
  fromCache?: boolean;
  isValid: boolean;
  transitionScore: number;
  busTransitHours: number;
  walkingHours: number;
  transitEfficiency: number;
  hasDirectBus?: boolean;
}

export interface BusLine {
  busNumber: string;
  direction: string;
  vehicleType?: string;
  fromStation: string;
  toStation: string;
  departureTime: string;
  arrivalTime: string;
}

export interface TransitSegment {
  fromLabel: string;
  boardingStation?: string;
  alightingStation?: string;
  walkingMinutes: number;
  departureTime: string;
  arrivalTime: string;
  transitEfficiency?: number;
  busLines: BusLine[];
}

export interface TripLeg {
  order: number;
  desId: number;
  destinationName: string;
  region: string;
  imageUrl?: string;
  lat?: number;
  lon?: number;
  arrivalTime: string;
  departureTime: string;
  stayDuration: string;
  transit: TransitSegment;
}

export interface MapPoint {
  order: number;
  label: string;
  lat: number;
  lon: number;
}

export interface ScoreTableStats {
  nodeCount: number;
  minuteCount?: number;
  hourCount: number;
  totalCells: number;
  validCells: number;
  validRatio: number;
  description: string;
}

export interface OptimizeResult {
  tripId?: number;
  tripName?: string;
  addressStart?: string;
  destinationCount: number;
  totalScore: number;
  timeUsed: number;
  timeAvailable: number;
  transitEfficiency: number;
  optimalRoute: Destination[];
  narrative?: string;
  legs?: TripLeg[];
  returnLeg?: TripLeg;
  mapPoints?: MapPoint[];
  scoreTableStats?: ScoreTableStats;
  pipelineTrace?: OptimizationStepTrace[];
  scoreTableCellTrace?: ScoreTableCellTrace[];
}

export interface TripItinerary {
  tripId: number;
  tripName: string;
  addressStart: string;
  destinationCount: number;
  totalScore: number;
  timeUsed: number;
  timeAvailable: number;
  transitEfficiency: number;
  narrative?: string;
  legs: TripLeg[];
  returnLeg?: TripLeg;
  mapPoints: MapPoint[];
}

// ─── Lookup models ────────────────────────────────────────────────────────────

export interface CategoryItem {
  categoriesId: number;
  categoriesName: string;
}

export interface DifficultyLevel {
  levelId: number;
  levelType: string;
}

export interface TravelerType {
  travelerId: number;
  typeTravelerName: string;
}

export interface FeatureType {
  featureId: number;
  feature: string;
}
