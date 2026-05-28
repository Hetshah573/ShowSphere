import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { adminApi } from '@/api';
import { PageLoader } from '@/components/Loading';
import { formatCurrency, formatDate, getStatusColor } from '@/lib/utils';
import { Users, Film, Ticket, DollarSign, TrendingUp, Calendar, CheckCircle, Clock, XCircle, AlertTriangle, ScanLine } from 'lucide-react';

export function AdminDashboard() {
  const { data, isLoading } = useQuery({
    queryKey: ['admin', 'dashboard'],
    queryFn: () => adminApi.getDashboardStats(),
  });

  if (isLoading) return <PageLoader />;

  const stats = data?.data;
  if (!stats) return <div className="text-center py-20 text-gray-500 dark:text-gray-400">Failed to load dashboard</div>;

  const maxRevenue = Math.max(...stats.dailyRevenue.map((d) => d.revenue), 1);

  return (
    <div className="max-w-7xl mx-auto px-4 py-8">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Admin Dashboard</h1>
        <div className="flex items-center gap-3">
          <Link to="/admin/verify" className="btn-secondary text-sm px-4 py-2 flex items-center gap-2">
            <ScanLine className="h-4 w-4" /> Ticket Scanner
          </Link>
          <Link to="/admin/movies" className="btn-primary text-sm px-4 py-2">
            Manage Movies
          </Link>
        </div>
      </div>

      {/* Main Stats Cards */}
      <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-6 gap-4 mb-8">
        <StatCard icon={<Users className="h-5 w-5" />} label="Users" value={stats.totalUsers.toString()} color="text-blue-500 bg-blue-500/10" />
        <StatCard icon={<Film className="h-5 w-5" />} label="Movies" value={stats.totalMovies.toString()} color="text-purple-500 bg-purple-500/10" />
        <StatCard icon={<Ticket className="h-5 w-5" />} label="Bookings" value={stats.totalBookings.toString()} color="text-green-500 bg-green-500/10" />
        <StatCard icon={<DollarSign className="h-5 w-5" />} label="Revenue" value={formatCurrency(stats.totalRevenue)} color="text-yellow-500 bg-yellow-500/10" />
        <StatCard icon={<TrendingUp className="h-5 w-5" />} label="Today" value={stats.todayBookings.toString()} color="text-pink-500 bg-pink-500/10" />
        <StatCard icon={<Calendar className="h-5 w-5" />} label="Shows" value={stats.upcomingShows.toString()} color="text-cyan-500 bg-cyan-500/10" />
      </div>

      {/* Today's Highlight + Booking Status */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        {/* Today's Revenue */}
        <div className="card p-6">
          <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-1">Today's Revenue</h3>
          <p className="text-3xl font-bold text-green-500">{formatCurrency(stats.todayRevenue)}</p>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">{stats.todayBookings} bookings today</p>
        </div>

        {/* Booking Status Breakdown */}
        <div className="card p-6">
          <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400 mb-3">Booking Status</h3>
          <div className="grid grid-cols-2 gap-3">
            <StatusPill icon={<CheckCircle className="h-4 w-4" />} label="Confirmed" count={stats.confirmedBookings} color="text-green-500" />
            <StatusPill icon={<Clock className="h-4 w-4" />} label="Pending" count={stats.pendingBookings} color="text-yellow-500" />
            <StatusPill icon={<XCircle className="h-4 w-4" />} label="Cancelled" count={stats.cancelledBookings} color="text-red-500" />
            <StatusPill icon={<AlertTriangle className="h-4 w-4" />} label="Expired" count={stats.expiredBookings} color="text-gray-500" />
          </div>
        </div>
      </div>

      {/* Revenue Chart + Top Movies */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
        {/* Revenue Chart (last 7 days) */}
        <div className="card p-6">
          <h3 className="font-semibold text-gray-900 dark:text-white mb-4">Revenue (Last 7 Days)</h3>
          <div className="flex items-end justify-between gap-2 h-40">
            {stats.dailyRevenue.map((day) => (
              <div key={day.date} className="flex-1 flex flex-col items-center gap-1">
                <span className="text-[10px] text-gray-500 dark:text-gray-400">
                  {day.revenue > 0 ? formatCurrency(day.revenue) : ''}
                </span>
                <div
                  className="w-full bg-primary-500/80 rounded-t-sm min-h-[4px] transition-all"
                  style={{ height: `${(day.revenue / maxRevenue) * 100}%` }}
                />
                <span className="text-[10px] text-gray-500 dark:text-gray-400">{day.date}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Top Movies */}
        <div className="card p-6">
          <h3 className="font-semibold text-gray-900 dark:text-white mb-4">Top Movies (by bookings)</h3>
          <div className="space-y-3">
            {stats.topMovies.length === 0 ? (
              <p className="text-sm text-gray-500">No confirmed bookings yet</p>
            ) : (
              stats.topMovies.map((movie, idx) => (
                <div key={movie.id} className="flex items-center gap-3">
                  <span className="text-sm font-bold text-gray-400 w-5">{idx + 1}</span>
                  {movie.posterUrl ? (
                    <img src={movie.posterUrl} alt="" className="w-8 h-11 object-cover rounded" />
                  ) : (
                    <div className="w-8 h-11 bg-gray-200 dark:bg-gray-700 rounded" />
                  )}
                  <div className="flex-1 min-w-0">
                    <p className="text-sm font-medium text-gray-900 dark:text-white truncate">{movie.title}</p>
                    <p className="text-xs text-gray-500">{movie.bookingCount} bookings • {formatCurrency(movie.revenue)}</p>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>

      {/* Recent Bookings Table */}
      <div className="card overflow-hidden">
        <div className="p-4 border-b border-gray-200 dark:border-gray-700">
          <h3 className="font-semibold text-gray-900 dark:text-white">Recent Bookings</h3>
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 dark:bg-gray-800/50">
              <tr>
                <th className="text-left p-3 text-gray-500 dark:text-gray-400 font-medium">Booking #</th>
                <th className="text-left p-3 text-gray-500 dark:text-gray-400 font-medium">User</th>
                <th className="text-left p-3 text-gray-500 dark:text-gray-400 font-medium">Movie</th>
                <th className="text-left p-3 text-gray-500 dark:text-gray-400 font-medium">Amount</th>
                <th className="text-left p-3 text-gray-500 dark:text-gray-400 font-medium">Status</th>
                <th className="text-left p-3 text-gray-500 dark:text-gray-400 font-medium">Date</th>
              </tr>
            </thead>
            <tbody>
              {stats.recentBookings.map((booking) => (
                <tr key={booking.id} className="border-t border-gray-200 dark:border-gray-700/50 hover:bg-gray-50 dark:hover:bg-gray-800/30">
                  <td className="p-3 font-mono text-xs">{booking.bookingNumber}</td>
                  <td className="p-3">{booking.userName}</td>
                  <td className="p-3">{booking.movieTitle}</td>
                  <td className="p-3">{formatCurrency(booking.totalAmount)}</td>
                  <td className="p-3">
                    <span className={`badge ${getStatusColor(booking.status)}`}>{booking.status}</span>
                  </td>
                  <td className="p-3 text-gray-500 dark:text-gray-400">{formatDate(booking.createdAt)}</td>
                </tr>
              ))}
              {stats.recentBookings.length === 0 && (
                <tr><td colSpan={6} className="p-6 text-center text-gray-500">No bookings yet</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

function StatCard({ icon, label, value, color }: { icon: React.ReactNode; label: string; value: string; color: string }) {
  return (
    <div className="card p-4">
      <div className={`w-9 h-9 rounded-lg flex items-center justify-center mb-2 ${color}`}>
        {icon}
      </div>
      <p className="text-xs text-gray-500 dark:text-gray-400">{label}</p>
      <p className="text-lg font-bold mt-0.5 text-gray-900 dark:text-white">{value}</p>
    </div>
  );
}

function StatusPill({ icon, label, count, color }: { icon: React.ReactNode; label: string; count: number; color: string }) {
  return (
    <div className="flex items-center gap-2">
      <span className={color}>{icon}</span>
      <span className="text-sm text-gray-700 dark:text-gray-300">{label}</span>
      <span className="ml-auto text-sm font-semibold text-gray-900 dark:text-white">{count}</span>
    </div>
  );
}
