import api from './client';
import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
  CreateMoviePayload,
  UpdateMoviePayload,
  AdminShow,
  Movie,
  MovieListItem,
  PagedResult,
  ShowsByMovie,
  SeatAvailability,
  Booking,
  BookingHistory,
  Review,
  DashboardStats,
  UserProfile,
  WishlistStatus,
  PaymentOrder,
  TicketVerification,
} from '@/types';

// Auth
export const authApi = {
  login: (data: LoginRequest) => api.post<AuthResponse>('/auth/login', data),
  register: (data: RegisterRequest) => api.post<AuthResponse>('/auth/register', data),
  googleLogin: (idToken: string) => api.post<AuthResponse>('/auth/google', { idToken }),
  refresh: (refreshToken: string) => api.post<AuthResponse>('/auth/refresh', { refreshToken }),
  logout: (refreshToken: string) => api.post('/auth/logout', { refreshToken }),
};

// Payments
export const paymentsApi = {
  createOrder: (bookingId: string) => api.post<PaymentOrder>('/payments/create-order', { bookingId }),
  verify: (data: { orderId: string; paymentId: string; signature: string }) => api.post<{ verified: boolean; transactionId: string }>('/payments/verify', data),
};

// Movies
export const moviesApi = {
  getAll: (params?: Record<string, string | number | boolean | undefined>) =>
    api.get<PagedResult<MovieListItem>>('/movies', { params }),
  getById: (id: string) => api.get<Movie>(`/movies/${id}`),
  getNowShowing: (city?: string, page = 1) =>
    api.get<PagedResult<MovieListItem>>('/movies/now-showing', { params: { city, page } }),
  getUpcoming: (page = 1) =>
    api.get<PagedResult<MovieListItem>>('/movies/upcoming', { params: { page } }),
  subscribeNotify: (movieId: string) => api.post<{ subscribed: boolean }>(`/movies/${movieId}/notify`),
  unsubscribeNotify: (movieId: string) => api.delete<{ subscribed: boolean }>(`/movies/${movieId}/notify`),
  getNotifyStatus: (movieId: string) => api.get<{ subscribed: boolean }>(`/movies/${movieId}/notify`),
};

// Shows
export const showsApi = {
  getByMovie: (movieId: string, city?: string, date?: string) =>
    api.get<ShowsByMovie[]>(`/shows/movie/${movieId}`, { params: { city, date } }),
};

// Bookings
export const bookingsApi = {
  getSeats: (showId: string) => api.get<SeatAvailability[]>(`/bookings/seats/${showId}`),
  create: (data: { showId: string; seatIds: string[]; paymentMethod: number }) =>
    api.post<Booking>('/bookings', data),
  confirm: (bookingId: string, transactionId: string) =>
    api.post<Booking>(`/bookings/${bookingId}/confirm`, { transactionId }),
  cancel: (bookingId: string) => api.post(`/bookings/${bookingId}/cancel`),
  getById: (bookingId: string) => api.get<Booking>(`/bookings/${bookingId}`),
  getHistory: (page = 1, pageSize = 10) =>
    api.get<PagedResult<BookingHistory>>('/bookings/history', { params: { page, pageSize } }),
};

// Reviews
export const reviewsApi = {
  getByMovie: (movieId: string, page = 1) =>
    api.get<PagedResult<Review>>(`/reviews/movie/${movieId}`, { params: { page } }),
  create: (data: { movieId: string; rating: number; comment?: string }) =>
    api.post<Review>('/reviews', data),
};

// Admin
export const adminApi = {
  getDashboardStats: () => api.get<DashboardStats>('/admin/dashboard/stats'),
  getGenres: () => api.get<{ id: number; name: string }[]>('/movies/genres'),
  createMovie: (data: CreateMoviePayload) => api.post('/movies', data),
  updateMovie: (id: string, data: UpdateMoviePayload) => api.put(`/movies/${id}`, data),
  deleteMovie: (id: string) => api.delete(`/movies/${id}`),
  // Shows
  getShows: (page = 1) => api.get<{ items: AdminShow[]; totalCount: number; page: number; totalPages: number }>('/shows/admin', { params: { page } }),
  createShow: (data: { movieId: string; screenId: string; startTime: string; basePrice: number }) => api.post('/shows', data),
  deleteShow: (id: string) => api.delete(`/shows/${id}`),
  // Theaters & Screens
  getTheaters: () => api.get<{ id: string; name: string; address: string; city: string; screenCount: number }[]>('/theaters'),
  getTheater: (id: string) => api.get<{ id: string; name: string; screens: { id: string; name: string; totalSeats: number; screenType: string }[] }>(`/theaters/${id}`),
};

// Users / Profile
export const usersApi = {
  getProfile: () => api.get<UserProfile>('/users/profile'),
  updateProfile: (data: { firstName: string; lastName: string; phone?: string }) =>
    api.put('/users/profile', data),
  changePassword: (data: { currentPassword: string; newPassword: string }) =>
    api.post('/users/change-password', data),
  forgotPassword: (email: string) => api.post('/users/forgot-password', { email }),
  resetPassword: (data: { token: string; newPassword: string }) =>
    api.post('/users/reset-password', data),
};

// Wishlist
export const wishlistApi = {
  getAll: (page = 1, pageSize = 20) =>
    api.get<PagedResult<MovieListItem>>('/wishlist', { params: { page, pageSize } }),
  isInWishlist: (movieId: string) => api.get<WishlistStatus>(`/wishlist/${movieId}`),
  add: (movieId: string) => api.post(`/wishlist/${movieId}`),
  remove: (movieId: string) => api.delete(`/wishlist/${movieId}`),
};

// Theaters (public)
export const theatersApi = {
  getAll: (city?: string) =>
    api.get<{ id: string; name: string; address: string; city: string; screenCount: number }[]>('/theaters', { params: { city } }),
};

// Ticket Verification (staff use)
export const verificationApi = {
  scan: (qrData: string) =>
    api.post<TicketVerification>('/verification/scan', { qrData }),
};
