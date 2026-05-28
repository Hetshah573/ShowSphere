import { Routes, Route } from 'react-router-dom';
import { Layout } from '@/components/Layout';
import { ProtectedRoute } from '@/routes/ProtectedRoute';
import { HomePage } from '@/pages/HomePage';
import { LoginPage } from '@/pages/LoginPage';
import { RegisterPage } from '@/pages/RegisterPage';
import { MoviesPage } from '@/pages/MoviesPage';
import { MovieDetailPage } from '@/pages/MovieDetailPage';
import { SeatSelectionPage } from '@/pages/SeatSelectionPage';
import { BookingConfirmPage } from '@/pages/BookingConfirmPage';
import { BookingHistoryPage } from '@/pages/BookingHistoryPage';
import { AdminDashboard } from '@/pages/AdminDashboard';
import { AdminMoviesPage } from '@/pages/AdminMoviesPage';
import { AdminShowsPage } from '@/pages/AdminShowsPage';
import { ProfilePage } from '@/pages/ProfilePage';
import { ForgotPasswordPage } from '@/pages/ForgotPasswordPage';
import { ResetPasswordPage } from '@/pages/ResetPasswordPage';
import { WishlistPage } from '@/pages/WishlistPage';
import { StaffVerificationPage } from '@/pages/StaffVerificationPage';

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<HomePage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />
        <Route path="/movies" element={<MoviesPage />} />
        <Route path="/movies/:id" element={<MovieDetailPage />} />
        <Route
          path="/booking/seats/:showId"
          element={
            <ProtectedRoute>
              <SeatSelectionPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/booking/:bookingId"
          element={
            <ProtectedRoute>
              <BookingConfirmPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/bookings"
          element={
            <ProtectedRoute>
              <BookingHistoryPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/profile"
          element={
            <ProtectedRoute>
              <ProfilePage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/wishlist"
          element={
            <ProtectedRoute>
              <WishlistPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin"
          element={
            <ProtectedRoute adminOnly>
              <AdminDashboard />
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin/movies"
          element={
            <ProtectedRoute adminOnly>
              <AdminMoviesPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin/shows"
          element={
            <ProtectedRoute adminOnly>
              <AdminShowsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin/verify"
          element={
            <ProtectedRoute adminOnly>
              <StaffVerificationPage />
            </ProtectedRoute>
          }
        />
      </Route>
    </Routes>
  );
}
