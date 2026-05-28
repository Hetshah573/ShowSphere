import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { bookingsApi } from '@/api';
import { PageLoader } from '@/components/Loading';
import { formatCurrency, formatDate, formatTime, getStatusColor } from '@/lib/utils';
import { Ticket, Calendar, MapPin } from 'lucide-react';

export function BookingHistoryPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ['bookings', 'history', page],
    queryFn: () => bookingsApi.getHistory(page),
  });

  if (isLoading) return <PageLoader />;

  const bookings = data?.data?.items || [];
  const totalPages = data?.data?.totalPages || 1;

  return (
    <div className="max-w-4xl mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-6 flex items-center gap-2">
        <Ticket className="h-6 w-6 text-primary-400" />
        My Bookings
      </h1>

      {bookings.length === 0 ? (
        <div className="text-center py-16">
          <Ticket className="h-16 w-16 text-gray-600 mx-auto mb-4" />
          <p className="text-gray-600 dark:text-gray-400 text-lg">No bookings yet.</p>
          <button onClick={() => navigate('/movies')} className="btn-primary mt-4">
            Browse Movies
          </button>
        </div>
      ) : (
        <div className="space-y-4">
          {bookings.map((booking) => (
            <div
              key={booking.id}
              onClick={() => navigate(`/booking/${booking.id}`)}
              className="card p-4 cursor-pointer hover:ring-2 hover:ring-primary-500/50 transition-all"
            >
              <div className="flex gap-4">
                {booking.moviePoster && (
                  <img
                    src={booking.moviePoster}
                    alt={booking.movieTitle}
                    className="w-16 h-24 object-cover rounded-lg flex-shrink-0"
                  />
                )}
                <div className="flex-1 min-w-0">
                  <div className="flex items-start justify-between gap-2">
                    <h3 className="font-semibold truncate">{booking.movieTitle}</h3>
                    <span className={`badge flex-shrink-0 ${getStatusColor(booking.status)}`}>
                      {booking.status}
                    </span>
                  </div>
                  <div className="mt-2 space-y-1 text-sm text-gray-600 dark:text-gray-400">
                    <p className="flex items-center gap-1">
                      <MapPin className="h-3 w-3" /> {booking.theaterName}
                    </p>
                    <p className="flex items-center gap-1">
                      <Calendar className="h-3 w-3" /> {formatDate(booking.showTime)} • {formatTime(booking.showTime)}
                    </p>
                  </div>
                  <div className="flex items-center justify-between mt-2">
                    <span className="text-xs text-gray-500">
                      {booking.totalSeats} seat{booking.totalSeats > 1 ? 's' : ''} • #{booking.bookingNumber}
                    </span>
                    <span className="font-semibold text-primary-400">
                      {formatCurrency(booking.totalAmount)}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-center gap-2 mt-8">
          <button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1} className="btn-secondary">
            Previous
          </button>
          <span className="flex items-center px-4 text-gray-600 dark:text-gray-400">Page {page} of {totalPages}</span>
          <button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages} className="btn-secondary">
            Next
          </button>
        </div>
      )}
    </div>
  );
}
