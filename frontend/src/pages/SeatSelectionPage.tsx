import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { bookingsApi } from '@/api';
import { PageLoader } from '@/components/Loading';
import { cn, formatCurrency } from '@/lib/utils';
import type { SeatAvailability } from '@/types';
import toast from 'react-hot-toast';

export function SeatSelectionPage() {
  const { showId } = useParams<{ showId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [selectedSeats, setSelectedSeats] = useState<SeatAvailability[]>([]);
  const [isBooking, setIsBooking] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ['seats', showId],
    queryFn: () => bookingsApi.getSeats(showId!),
    enabled: !!showId,
    refetchInterval: 60000, // Polling fallback; SignalR is primary
  });

  useEffect(() => {
    if (!showId) return;

    const resolveHubUrl = () => {
      if (import.meta.env.VITE_SIGNALR_URL) return import.meta.env.VITE_SIGNALR_URL;

      const apiUrl = import.meta.env.VITE_API_URL || '/api';
      if (apiUrl.startsWith('http'))
        return `${apiUrl.replace(/\/api\/?$/, '')}/hubs/seats`;

      return `${window.location.origin}/hubs/seats`;
    };

    const connection = new HubConnectionBuilder()
      .withUrl(resolveHubUrl())
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Warning)
      .build();

    connection.on('SeatUpdated', (seatId: string, isAvailable: boolean, isLocked: boolean = false) => {
      queryClient.setQueryData(['seats', showId], (old: unknown) => {
        const oldResponse = old as { data?: SeatAvailability[] } | undefined;
        if (!oldResponse?.data) return old;

        return {
          ...oldResponse,
          data: oldResponse.data.map((seat) =>
            seat.seatId === seatId
              ? {
                  ...seat,
                  isAvailable,
                  isLocked,
                }
              : seat
          ),
        };
      });

      // Drop seat from selection immediately if another event made it unavailable.
      if (!isAvailable) {
        setSelectedSeats((prev) => prev.filter((s) => s.seatId !== seatId));
      }
    });

    const start = async () => {
      try {
        await connection.start();
        await connection.invoke('JoinShowGroup', showId);
      } catch {
        // Polling fallback still keeps seat state fresh.
      }
    };

    start();

    return () => {
      const stop = async () => {
        try {
          if (connection.state === 'Connected') {
            await connection.invoke('LeaveShowGroup', showId);
          }
        } finally {
          await connection.stop();
        }
      };

      void stop();
    };
  }, [queryClient, showId]);

  if (isLoading) return <PageLoader />;

  const seats = data?.data || [];
  const rows = [...new Set(seats.map((s) => s.row))];

  const toggleSeat = (seat: SeatAvailability) => {
    if (!seat.isAvailable) return;
    setSelectedSeats((prev) =>
      prev.find((s) => s.seatId === seat.seatId)
        ? prev.filter((s) => s.seatId !== seat.seatId)
        : prev.length < 10
        ? [...prev, seat]
        : prev
    );
  };

  const getSeatColor = (seat: SeatAvailability) => {
    if (selectedSeats.find((s) => s.seatId === seat.seatId)) return 'bg-primary-500 text-white';
    if (!seat.isAvailable) return 'bg-gray-300 dark:bg-gray-700 text-gray-500 cursor-not-allowed';
    if (seat.isLocked) return 'bg-yellow-600/50 text-yellow-300 cursor-not-allowed';
    switch (seat.category) {
      case 'Silver': return 'bg-gray-600 hover:bg-gray-500 text-gray-200';
      case 'Gold': return 'bg-yellow-900/50 hover:bg-yellow-800/50 text-yellow-200';
      case 'Platinum': return 'bg-purple-900/50 hover:bg-purple-800/50 text-purple-200';
      case 'Recliner': return 'bg-red-900/50 hover:bg-red-800/50 text-red-200';
      default: return 'bg-gray-600 hover:bg-gray-500';
    }
  };

  const totalAmount = selectedSeats.reduce((sum, s) => sum + s.price, 0);

  const handleBooking = async () => {
    if (selectedSeats.length === 0) {
      toast.error('Please select at least one seat');
      return;
    }
    setIsBooking(true);
    try {
      const response = await bookingsApi.create({
        showId: showId!,
        seatIds: selectedSeats.map((s) => s.seatId),
        paymentMethod: 0, // Credit Card
      });
      navigate(`/booking/${response.data.id}`);
    } catch (err: unknown) {
      const error = err as { response?: { data?: { error?: string } } };
      toast.error(error.response?.data?.error || 'Booking failed. Seats may have been taken.');
    } finally {
      setIsBooking(false);
    }
  };

  return (
    <div className="max-w-5xl mx-auto px-4 py-8">
      <h1 className="text-2xl font-bold mb-6">Select Your Seats</h1>

      {/* Screen indicator */}
      <div className="text-center mb-8">
        <div className="w-3/4 mx-auto h-2 bg-gradient-to-r from-transparent via-primary-400 to-transparent rounded-full mb-2" />
        <p className="text-xs text-gray-500 uppercase tracking-wider">Screen</p>
      </div>

      {/* Seat Map */}
      <div className="overflow-x-auto mb-8">
        <div className="min-w-[600px]">
          {rows.map((row) => {
            const rowSeats = seats.filter((s) => s.row === row);
            return (
              <div key={row} className="flex items-center gap-1 mb-1.5 justify-center">
                <span className="w-6 text-xs text-gray-500 text-right mr-2">{row}</span>
                {rowSeats.map((seat) => (
                  <button
                    key={seat.seatId}
                    onClick={() => toggleSeat(seat)}
                    disabled={!seat.isAvailable || seat.isLocked}
                    className={cn(
                      'w-7 h-7 rounded text-[10px] font-medium transition-all flex items-center justify-center',
                      getSeatColor(seat)
                    )}
                    title={`${seat.row}${seat.number} - ${seat.category} - ${formatCurrency(seat.price)}`}
                  >
                    {seat.number}
                  </button>
                ))}
              </div>
            );
          })}
        </div>
      </div>

      {/* Legend */}
      <div className="flex flex-wrap justify-center gap-4 mb-8 text-sm">
        <div className="flex items-center gap-2">
          <div className="w-5 h-5 bg-gray-600 rounded" />
          <span className="text-gray-600 dark:text-gray-400">Available</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-5 h-5 bg-primary-500 rounded" />
          <span className="text-gray-600 dark:text-gray-400">Selected</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-5 h-5 bg-gray-300 dark:bg-gray-700 rounded" />
          <span className="text-gray-600 dark:text-gray-400">Booked</span>
        </div>
        <div className="flex items-center gap-2">
          <div className="w-5 h-5 bg-yellow-600/50 rounded" />
          <span className="text-gray-600 dark:text-gray-400">Locked</span>
        </div>
      </div>

      {/* Category Legend - Dynamic from actual seat data */}
      <div className="flex flex-wrap justify-center gap-4 mb-8 text-xs">
        {(() => {
          const categories = [...new Map(seats.map(s => [s.category, s.price])).entries()];
          const categoryColors: Record<string, string> = {
            Silver: 'text-gray-300',
            Gold: 'text-yellow-300',
            Platinum: 'text-purple-300',
            Recliner: 'text-red-300',
          };
          return categories.map(([cat, price]) => (
            <span key={cat} className={categoryColors[cat] || 'text-gray-400'}>
              {cat}: {formatCurrency(price)}
            </span>
          ));
        })()}
      </div>

      {/* Booking Summary */}
      {selectedSeats.length > 0 && (
        <div className="card p-4 sticky bottom-4">
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600 dark:text-gray-400">
                {selectedSeats.length} seat{selectedSeats.length > 1 ? 's' : ''} selected
              </p>
              <p className="text-xs text-gray-500">
                {selectedSeats.map((s) => `${s.row}${s.number}`).join(', ')}
              </p>
            </div>
            <div className="text-right">
              <p className="text-lg font-bold">{formatCurrency(totalAmount)}</p>
              <button
                onClick={handleBooking}
                disabled={isBooking}
                className="btn-primary mt-2"
              >
                {isBooking ? 'Booking...' : 'Book Now'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
