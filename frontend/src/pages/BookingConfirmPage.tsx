import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { bookingsApi, paymentsApi } from '@/api';
import { PageLoader } from '@/components/Loading';
import { formatCurrency, formatDate, formatTime, getStatusColor } from '@/lib/utils';
import { CheckCircle, XCircle, Clock, Ticket, Download } from 'lucide-react';
import { useState, useEffect, useRef } from 'react';
import toast from 'react-hot-toast';
import { QRCodeSVG } from 'qrcode.react';

export function BookingConfirmPage() {
  const { bookingId } = useParams<{ bookingId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [isConfirming, setIsConfirming] = useState(false);
  const [isCancelling, setIsCancelling] = useState(false);
  const [timeLeft, setTimeLeft] = useState<number | null>(null);
  const ticketRef = useRef<HTMLDivElement>(null);

  const { data, isLoading, refetch } = useQuery({
    queryKey: ['booking', bookingId],
    queryFn: () => bookingsApi.getById(bookingId!),
    enabled: !!bookingId,
  });

  const booking = data?.data;

  // Countdown timer for pending bookings
  useEffect(() => {
    if (!booking || booking.status !== 'Pending' || !booking.expiresAt) return;

    let interval: ReturnType<typeof setInterval>;

    const updateTimer = () => {
      const expires = new Date(booking.expiresAt!).getTime();
      const now = Date.now();
      const remaining = Math.max(0, Math.floor((expires - now) / 1000));
      setTimeLeft(remaining);
      if (remaining <= 0) {
        clearInterval(interval);
        refetch(); // backend will now return Expired status on this GET
      }
    };

    updateTimer();
    interval = setInterval(updateTimer, 1000);
    return () => clearInterval(interval);
  }, [booking, refetch]);

  const handleConfirmPayment = async () => {
    setIsConfirming(true);
    try {
      // Step 1: Create order on backend (calls Razorpay API)
      const orderRes = await paymentsApi.createOrder(bookingId!);
      const order = orderRes.data;

      // Step 2: Load Razorpay script if not loaded
      if (!window.Razorpay) {
        await new Promise<void>((resolve, reject) => {
          const script = document.createElement('script');
          script.src = 'https://checkout.razorpay.com/v1/checkout.js';
          script.onload = () => resolve();
          script.onerror = () => reject(new Error('Failed to load Razorpay'));
          document.body.appendChild(script);
        });
      }

      // Step 3: Open Razorpay checkout
      const rzp = new window.Razorpay({
        key: order.gatewayKey,
        amount: order.amount * 100,
        currency: order.currency,
        name: 'ShowSphere',
        description: `Booking: ${order.bookingNumber}`,
        order_id: order.orderId,
        prefill: {
          name: order.customerName,
          email: order.customerEmail,
        },
        theme: { color: '#6366f1' },
        handler: async (response: RazorpayResponse) => {
          try {
            // Step 4: Verify payment signature on backend
            const verifyRes = await paymentsApi.verify({
              orderId: response.razorpay_order_id,
              paymentId: response.razorpay_payment_id,
              signature: response.razorpay_signature,
            });

            if (verifyRes.data.verified) {
              // Step 5: Confirm booking with verified transaction ID
              await bookingsApi.confirm(bookingId!, response.razorpay_payment_id);
              toast.success('Payment successful! Enjoy your movie!');
              queryClient.invalidateQueries({ queryKey: ['bookings'] });
              refetch();
            } else {
              toast.error('Payment verification failed');
            }
          } catch {
            toast.error('Payment verification failed');
          } finally {
            setIsConfirming(false);
          }
        },
        modal: {
          ondismiss: () => {
            setIsConfirming(false);
            toast.error('Payment cancelled');
          },
        },
      });

      rzp.open();
    } catch (err: unknown) {
      const error = err as { response?: { data?: { error?: string } } };
      toast.error(error.response?.data?.error || 'Failed to initiate payment');
      setIsConfirming(false);
    }
  };

  const handleCancel = async () => {
    if (!confirm('Are you sure you want to cancel this booking?')) return;
    setIsCancelling(true);
    try {
      await bookingsApi.cancel(bookingId!);
      toast.success('Booking cancelled');
      queryClient.invalidateQueries({ queryKey: ['bookings'] });
      refetch();
    } catch (err: unknown) {
      const error = err as { response?: { data?: { error?: string } } };
      toast.error(error.response?.data?.error || 'Cancellation failed');
    } finally {
      setIsCancelling(false);
    }
  };

  const handleDownloadPDF = () => {
    if (!ticketRef.current || !booking) return;
    const printWindow = window.open('', '_blank');
    if (!printWindow) {
      toast.error('Please allow popups to download ticket');
      return;
    }
    const qrSvg = ticketRef.current.querySelector('svg');
    const qrHtml = qrSvg ? qrSvg.outerHTML : '';
    printWindow.document.write(`
      <html>
      <head><title>ShowSphere Ticket - ${booking.bookingNumber}</title>
      <style>
        body { font-family: Arial, sans-serif; padding: 40px; max-width: 600px; margin: auto; }
        .header { text-align: center; border-bottom: 2px solid #6366f1; padding-bottom: 20px; margin-bottom: 20px; }
        .header h1 { color: #6366f1; margin: 0; }
        .details { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-bottom: 20px; }
        .details .label { color: #666; font-size: 12px; }
        .details .value { font-weight: bold; font-size: 14px; }
        .seats { border-top: 1px solid #ddd; padding-top: 16px; }
        .seat-row { display: flex; justify-content: space-between; padding: 4px 0; font-size: 13px; }
        .qr { text-align: center; margin-top: 20px; }
        .total { font-size: 18px; font-weight: bold; color: #6366f1; text-align: right; margin-top: 12px; }
        @media print { body { padding: 20px; } }
      </style>
      </head>
      <body>
        <div class="header">
          <h1>🎬 ShowSphere</h1>
          <p style="color:#666; margin:4px 0;">Movie Ticket - ${booking.bookingNumber}</p>
        </div>
        <div class="details">
          <div><div class="label">Movie</div><div class="value">${booking.movieTitle}</div></div>
          <div><div class="label">Theater</div><div class="value">${booking.theaterName}</div></div>
          <div><div class="label">Screen</div><div class="value">${booking.screenName}</div></div>
          <div><div class="label">Show Time</div><div class="value">${formatDate(booking.showTime)} • ${formatTime(booking.showTime)}</div></div>
          <div><div class="label">Seats</div><div class="value">${booking.seats.map(s => s.row + s.number).join(', ')}</div></div>
          <div><div class="label">Status</div><div class="value">${booking.status}</div></div>
        </div>
        <div class="seats">
          <p style="color:#666; font-size:12px; margin-bottom:8px;">Seat Breakdown</p>
          ${booking.seats.map(s => `<div class="seat-row"><span>${s.row}${s.number} (${s.category})</span><span>${formatCurrency(s.price)}</span></div>`).join('')}
          <div class="total">Total: ${formatCurrency(booking.totalAmount)}</div>
        </div>
        <div class="qr">${qrHtml}</div>
        <script>window.print(); window.close();</script>
      </body></html>
    `);
    printWindow.document.close();
  };

  const formatCountdown = (seconds: number) => {
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${m}:${s.toString().padStart(2, '0')}`;
  };

  if (isLoading) return <PageLoader />;
  if (!booking) return <div className="text-center py-20 text-gray-600 dark:text-gray-400">Booking not found</div>;

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      {/* Status Header */}
      <div className="text-center mb-8">
        {booking.status === 'Confirmed' && (
          <>
            <CheckCircle className="h-16 w-16 text-green-400 mx-auto mb-3" />
            <h1 className="text-2xl font-bold text-green-400">Booking Confirmed!</h1>
          </>
        )}
        {booking.status === 'Pending' && (
          <>
            <Clock className="h-16 w-16 text-yellow-400 mx-auto mb-3" />
            <h1 className="text-2xl font-bold text-yellow-400">Pending Payment</h1>
            {timeLeft !== null && timeLeft > 0 && (
              <p className={`text-2xl font-mono mt-2 ${timeLeft <= 60 ? 'text-red-400 animate-pulse' : 'text-yellow-300'}`}>
                ⏱ {formatCountdown(timeLeft)}
              </p>
            )}
            {timeLeft === 0 && (
              <p className="text-red-400 mt-1 font-semibold">Booking expired!</p>
            )}
            {timeLeft !== null && timeLeft > 0 && (
              <p className="text-gray-600 dark:text-gray-400 mt-1 text-sm">Complete payment before time runs out</p>
            )}
          </>
        )}
        {booking.status === 'Cancelled' && (
          <>
            <XCircle className="h-16 w-16 text-red-400 mx-auto mb-3" />
            <h1 className="text-2xl font-bold text-red-400">Booking Cancelled</h1>
          </>
        )}
      </div>

      {/* Booking Details */}
      <div className="card p-6 space-y-4">
        <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 pb-4">
          <div className="flex items-center gap-2">
            <Ticket className="h-5 w-5 text-primary-400" />
            <span className="font-mono text-sm">{booking.bookingNumber}</span>
          </div>
          <span className={`badge ${getStatusColor(booking.status)}`}>{booking.status}</span>
        </div>

        <div className="grid grid-cols-2 gap-4 text-sm">
          <div>
            <p className="text-gray-600 dark:text-gray-400">Movie</p>
            <p className="font-semibold">{booking.movieTitle}</p>
          </div>
          <div>
            <p className="text-gray-600 dark:text-gray-400">Theater</p>
            <p className="font-semibold">{booking.theaterName}</p>
          </div>
          <div>
            <p className="text-gray-600 dark:text-gray-400">Screen</p>
            <p className="font-semibold">{booking.screenName}</p>
          </div>
          <div>
            <p className="text-gray-600 dark:text-gray-400">Show Time</p>
            <p className="font-semibold">{formatDate(booking.showTime)} • {formatTime(booking.showTime)}</p>
          </div>
          <div>
            <p className="text-gray-600 dark:text-gray-400">Seats</p>
            <p className="font-semibold">{booking.seats.map((s) => `${s.row}${s.number}`).join(', ')}</p>
          </div>
          <div>
            <p className="text-gray-600 dark:text-gray-400">Total Amount</p>
            <p className="font-semibold text-lg text-primary-600 dark:text-primary-400">{formatCurrency(booking.totalAmount)}</p>
          </div>
        </div>

        {/* Seat Breakdown */}
        <div className="border-t border-gray-200 dark:border-gray-700 pt-4">
          <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">Seat Breakdown</p>
          <div className="space-y-1">
            {booking.seats.map((seat) => (
              <div key={seat.seatId} className="flex justify-between text-sm">
                <span>{seat.row}{seat.number} ({seat.category})</span>
                <span>{formatCurrency(seat.price)}</span>
              </div>
            ))}
          </div>
        </div>

        {/* QR Code */}
        {booking.status === 'Confirmed' && booking.qrCode && (
          <div ref={ticketRef} className="border-t border-gray-200 dark:border-gray-700 pt-4 text-center">
            <p className="text-sm text-gray-600 dark:text-gray-400 mb-2">Ticket QR Code</p>
            <div className="bg-white p-4 rounded-lg inline-block">
              <QRCodeSVG value={booking.qrCode} size={180} />
            </div>
          </div>
        )}

        {/* Actions */}
        <div className="border-t border-gray-200 dark:border-gray-700 pt-4 flex flex-wrap gap-3">
          {booking.status === 'Pending' && (
            <button onClick={handleConfirmPayment} disabled={isConfirming} className="btn-primary flex-1">
              {isConfirming ? 'Processing...' : 'Confirm Payment'}
            </button>
          )}
          {(booking.status === 'Pending' || booking.status === 'Confirmed') && (
            <button onClick={handleCancel} disabled={isCancelling} className="btn-danger">
              {isCancelling ? 'Cancelling...' : 'Cancel Booking'}
            </button>
          )}
          {booking.status === 'Confirmed' && booking.qrCode && (
            <button onClick={handleDownloadPDF} className="btn-secondary flex items-center gap-2">
              <Download className="h-4 w-4" /> Download Ticket
            </button>
          )}
          <button onClick={() => navigate('/bookings')} className="btn-secondary">
            My Bookings
          </button>
        </div>
      </div>
    </div>
  );
}
