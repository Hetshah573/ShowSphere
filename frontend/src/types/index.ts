export interface User {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  phone?: string;
  role: string;
}

export interface PaymentOrder {
  orderId: string;
  amount: number;
  currency: string;
  gatewayKey: string;
  provider: string;
  bookingNumber: string;
  customerName: string;
  customerEmail: string;
}

export interface AuthResponse {
  userId: string;
  email: string;
  phone?: string;
  firstName: string;
  lastName: string;
  role: string;
  accessToken: string;
  refreshToken: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phone?: string;
}

export interface Movie {
  id: string;
  title: string;
  description: string;
  posterUrl: string | null;
  trailerUrl: string | null;
  durationMinutes: number;
  language: string;
  certificate: string;
  releaseDate: string;
  averageRating: number;
  totalReviews: number;
  genres: string[];
  cast: CastMember[];
}

export interface MovieListItem {
  id: string;
  title: string;
  posterUrl: string | null;
  durationMinutes: number;
  language: string;
  certificate: string;
  releaseDate: string;
  averageRating: number;
  genres: string[];
}

export interface CastMember {
  id: string;
  name: string;
  role: string;
  photoUrl: string | null;
}

export interface Theater {
  id: string;
  name: string;
  address: string;
  city: string;
  state: string;
  pinCode: string;
  screens: Screen[];
}

export interface TheaterList {
  id: string;
  name: string;
  address: string;
  city: string;
  screenCount: number;
}

export interface Screen {
  id: string;
  name: string;
  totalSeats: number;
  screenType: string;
}

export interface ShowsByMovie {
  theaterName: string;
  theaterAddress: string;
  city: string;
  shows: ShowTiming[];
}

export interface ShowTiming {
  showId: string;
  screenName: string;
  screenType: string;
  startTime: string;
  basePrice: number;
  availableSeats: number;
}

export interface SeatAvailability {
  seatId: string;
  row: string;
  number: number;
  category: string;
  price: number;
  isAvailable: boolean;
  isLocked: boolean;
}

export interface Booking {
  id: string;
  bookingNumber: string;
  movieTitle: string;
  theaterName: string;
  screenName: string;
  showTime: string;
  totalSeats: number;
  totalAmount: number;
  status: string;
  seats: BookingSeat[];
  qrCode: string | null;
  bookedAt: string;
  expiresAt: string | null;
}

export interface BookingSeat {
  seatId: string;
  row: string;
  number: number;
  category: string;
  price: number;
}

export interface BookingHistory {
  id: string;
  bookingNumber: string;
  movieTitle: string;
  moviePoster: string | null;
  theaterName: string;
  showTime: string;
  totalSeats: number;
  totalAmount: number;
  status: string;
  bookedAt: string;
}

export interface Review {
  id: string;
  userId: string;
  userName: string;
  rating: number;
  comment: string | null;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface DashboardStats {
  totalUsers: number;
  totalMovies: number;
  totalBookings: number;
  totalRevenue: number;
  todayBookings: number;
  todayRevenue: number;
  confirmedBookings: number;
  pendingBookings: number;
  cancelledBookings: number;
  expiredBookings: number;
  upcomingShows: number;
  topMovies: TopMovie[];
  dailyRevenue: DailyRevenue[];
  recentBookings: RecentBooking[];
}

export interface TopMovie {
  id: string;
  title: string;
  posterUrl: string | null;
  bookingCount: number;
  revenue: number;
}

export interface DailyRevenue {
  date: string;
  revenue: number;
}

export interface RecentBooking {
  id: string;
  bookingNumber: string;
  userName: string;
  movieTitle: string;
  totalAmount: number;
  status: string;
  createdAt: string;
}

export interface UserProfile {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  phone: string | null;
  role: string;
  createdAt: string;
  totalBookings: number;
  totalReviews: number;
}

export interface WishlistStatus {
  isInWishlist: boolean;
}

export interface CreateMoviePayload {
  title: string;
  description: string;
  posterUrl?: string;
  trailerUrl?: string;
  durationMinutes: number;
  language: string;
  certificate: number;
  releaseDate: string;
  genreIds: number[];
  cast: { castId: string; role: string }[];
}

export interface UpdateMoviePayload {
  title: string;
  description: string;
  posterUrl?: string;
  trailerUrl?: string;
  durationMinutes: number;
  language: string;
  certificate: number;
  releaseDate: string;
  isActive: boolean;
  genreIds: number[];
}

export interface AdminShow {
  id: string;
  movieId: string;
  movieTitle: string;
  screenId: string;
  screenName: string;
  theaterName: string;
  city: string;
  startTime: string;
  endTime: string;
  basePrice: number;
  availableSeats: number;
  totalSeats: number;
}

export interface VerifiedSeat {
  row: string;
  number: number;
  category: string;
  price: number;
}

export interface TicketVerification {
  bookingNumber: string;
  bookingStatus: string;
  customerName: string;
  customerEmail: string;
  movieTitle: string;
  posterUrl: string | null;
  theaterName: string;
  theaterAddress: string;
  city: string;
  screenName: string;
  screenType: string;
  showTime: string;
  seats: VerifiedSeat[];
  totalAmount: number;
  paymentMethod: string;
  transactionId: string | null;
  bookedAt: string;
  isScanned: boolean;
  scannedAt: string | null;
  /** "VALID" | "ALREADY_USED" | "INVALID" */
  verificationStatus: string;
  verificationMessage: string;
}
