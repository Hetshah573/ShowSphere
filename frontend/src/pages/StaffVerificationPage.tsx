import { useState, useRef } from 'react';
import { useMutation } from '@tanstack/react-query';
import { verificationApi } from '@/api';
import { formatCurrency, formatDate, formatTime } from '@/lib/utils';
import type { TicketVerification } from '@/types';
import { QrCameraScanner } from '@/components/QrCameraScanner';
import {
  CheckCircle,
  XCircle,
  AlertTriangle,
  ScanLine,
  User,
  Film,
  MapPin,
  Clock,
  CreditCard,
  QrCode,
  RotateCcw,
  Camera,
} from 'lucide-react';

// ─── Helpers ──────────────────────────────────────────────────────────────────

function StatusBadge({ status }: { status: string }) {
  if (status === 'VALID') {
    return (
      <div className="flex items-center gap-3 px-6 py-4 bg-green-50 dark:bg-green-900/30 border border-green-300 dark:border-green-700 rounded-xl">
        <CheckCircle className="h-10 w-10 text-green-500 flex-shrink-0" />
        <div>
          <p className="text-xl font-bold text-green-700 dark:text-green-300">VALID — Entry Granted</p>
          <p className="text-sm text-green-600 dark:text-green-400">Ticket accepted. Marked as used.</p>
        </div>
      </div>
    );
  }
  if (status === 'ALREADY_USED') {
    return (
      <div className="flex items-center gap-3 px-6 py-4 bg-yellow-50 dark:bg-yellow-900/30 border border-yellow-300 dark:border-yellow-700 rounded-xl">
        <AlertTriangle className="h-10 w-10 text-yellow-500 flex-shrink-0" />
        <div>
          <p className="text-xl font-bold text-yellow-700 dark:text-yellow-300">ALREADY USED</p>
          <p className="text-sm text-yellow-600 dark:text-yellow-400">This ticket was already scanned.</p>
        </div>
      </div>
    );
  }
  return (
    <div className="flex items-center gap-3 px-6 py-4 bg-red-50 dark:bg-red-900/30 border border-red-300 dark:border-red-700 rounded-xl">
      <XCircle className="h-10 w-10 text-red-500 flex-shrink-0" />
      <div>
        <p className="text-xl font-bold text-red-700 dark:text-red-300">INVALID</p>
        <p className="text-sm text-red-600 dark:text-red-400">This ticket is not valid for entry.</p>
      </div>
    </div>
  );
}

function InfoRow({ label, value }: { label: string; value: string | null | undefined }) {
  if (!value) return null;
  return (
    <div className="flex justify-between py-2 border-b border-gray-100 dark:border-gray-700 last:border-0 text-sm">
      <span className="text-gray-500 dark:text-gray-400">{label}</span>
      <span className="font-medium text-right max-w-[60%]">{value}</span>
    </div>
  );
}

// ─── Main Page ────────────────────────────────────────────────────────────────

export function StaffVerificationPage() {
  const [qrInput, setQrInput] = useState('');
  const [result, setResult]   = useState<TicketVerification | null>(null);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [showCamera, setShowCamera] = useState(false);
  const inputRef = useRef<HTMLTextAreaElement>(null);

  const { mutate, isPending } = useMutation({
    mutationFn: (qrData: string) => verificationApi.scan(qrData),
    onSuccess: (res) => {
      setResult(res.data);
      setErrorMsg(null);
    },
    onError: (err: { response?: { data?: { error?: string } } }) => {
      setResult(null);
      setErrorMsg(err.response?.data?.error ?? 'Verification failed. Please check the QR data.');
    },
  });

  const handleVerify = () => {
    const trimmed = qrInput.trim();
    if (!trimmed) return;
    mutate(trimmed);
  };

  // Called when camera scanner successfully decodes a QR
  const handleCameraScan = (decoded: string) => {
    setShowCamera(false);
    setQrInput(decoded);
    setResult(null);
    setErrorMsg(null);
    mutate(decoded);
  };

  const handleReset = () => {
    setQrInput('');
    setResult(null);
    setErrorMsg(null);
    setTimeout(() => inputRef.current?.focus(), 50);
  };

  // Allow pressing Enter in the textarea to trigger verify (Shift+Enter for newline)
  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleVerify();
    }
  };

  return (
    <div className="max-w-2xl mx-auto px-4 py-8">
      {/* Camera Scanner Modal */}
      {showCamera && (
        <QrCameraScanner
          onScan={handleCameraScan}
          onClose={() => setShowCamera(false)}
        />
      )}
      {/* Header */}
      <div className="text-center mb-8">
        <div className="inline-flex items-center justify-center w-16 h-16 bg-primary-100 dark:bg-primary-900/30 rounded-full mb-4">
          <ScanLine className="h-8 w-8 text-primary-600 dark:text-primary-400" />
        </div>
        <h1 className="text-2xl font-bold">Ticket Scanner</h1>
        <p className="text-gray-600 dark:text-gray-400 mt-1 text-sm">
          Staff verification portal — scan or paste QR code data to validate a ticket
        </p>
      </div>

      {/* Input Card */}
      <div className="card p-6 mb-6">
        <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
          QR Code Data
        </label>
        <p className="text-xs text-gray-500 dark:text-gray-400 mb-3">
          Paste the decoded QR text here (format: <code className="bg-gray-100 dark:bg-gray-800 px-1 rounded">SHOWSPHERE|BK-XXXXX|&lt;signature&gt;</code>), or connect a barcode scanner keyboard wedge — it will auto-type into this field.
        </p>
        <textarea
          ref={inputRef}
          value={qrInput}
          onChange={(e) => setQrInput(e.target.value)}
          onKeyDown={handleKeyDown}
          rows={3}
          placeholder="SHOWSPHERE|SS20260528ABCD1234|a1b2c3..."
          className="w-full px-3 py-2 border border-gray-300 dark:border-gray-600 rounded-lg bg-white dark:bg-gray-800 text-sm font-mono focus:outline-none focus:ring-2 focus:ring-primary-500 resize-none"
          autoFocus
        />
        <div className="flex gap-3 mt-4">
          <button
            onClick={handleVerify}
            disabled={isPending || !qrInput.trim()}
            className="btn-primary flex items-center gap-2 flex-1"
          >
            <QrCode className="h-4 w-4" />
            {isPending ? 'Verifying...' : 'Verify Ticket'}
          </button>
          <button
            onClick={() => setShowCamera(true)}
            disabled={isPending}
            title="Scan with camera"
            className="btn-secondary flex items-center gap-2 px-4"
          >
            <Camera className="h-4 w-4" />
            <span className="hidden sm:inline">Camera</span>
          </button>
          {(result || errorMsg) && (
            <button onClick={handleReset} className="btn-secondary flex items-center gap-2">
              <RotateCcw className="h-4 w-4" /> Reset
            </button>
          )}
        </div>
      </div>

      {/* Error state */}
      {errorMsg && !result && (
        <div className="flex items-start gap-3 px-5 py-4 bg-red-50 dark:bg-red-900/30 border border-red-300 dark:border-red-700 rounded-xl mb-6">
          <XCircle className="h-5 w-5 text-red-500 mt-0.5 flex-shrink-0" />
          <p className="text-red-700 dark:text-red-300 text-sm">{errorMsg}</p>
        </div>
      )}

      {/* Verification Result */}
      {result && (
        <div className="space-y-5">
          {/* Verdict banner */}
          <StatusBadge status={result.verificationStatus} />

          {/* Already used detail */}
          {result.verificationStatus === 'ALREADY_USED' && result.scannedAt && (
            <p className="text-center text-sm text-yellow-600 dark:text-yellow-400">
              First scanned on {formatDate(result.scannedAt)} at {formatTime(result.scannedAt)}
            </p>
          )}

          {/* Movie + Poster */}
          <div className="card p-5 flex gap-4">
            {result.posterUrl && (
              <img
                src={result.posterUrl}
                alt={result.movieTitle}
                className="w-20 h-28 object-cover rounded-lg flex-shrink-0"
              />
            )}
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-2 mb-1">
                <Film className="h-4 w-4 text-primary-500 flex-shrink-0" />
                <h2 className="text-lg font-bold truncate">{result.movieTitle}</h2>
              </div>
              <div className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400 mb-1">
                <MapPin className="h-4 w-4 flex-shrink-0" />
                <span className="truncate">{result.theaterName} — {result.screenName} ({result.screenType})</span>
              </div>
              <div className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400">
                <Clock className="h-4 w-4 flex-shrink-0" />
                <span>{formatDate(result.showTime)} · {formatTime(result.showTime)}</span>
              </div>
              <p className="text-xs text-gray-400 mt-1">{result.city} · {result.theaterAddress}</p>
            </div>
          </div>

          {/* Booking Details */}
          <div className="card p-5">
            <h3 className="font-semibold text-sm text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-3">
              Booking Details
            </h3>
            <InfoRow label="Booking Ref" value={result.bookingNumber} />
            <InfoRow label="Status" value={result.bookingStatus} />
            <InfoRow label="Booked On" value={`${formatDate(result.bookedAt)} · ${formatTime(result.bookedAt)}`} />
            <InfoRow label="Total Amount" value={formatCurrency(result.totalAmount)} />
            <InfoRow label="Payment" value={result.paymentMethod} />
            <InfoRow label="Transaction ID" value={result.transactionId} />
          </div>

          {/* Customer */}
          <div className="card p-5">
            <h3 className="font-semibold text-sm text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-3 flex items-center gap-2">
              <User className="h-4 w-4" /> Customer
            </h3>
            <InfoRow label="Name"  value={result.customerName} />
            <InfoRow label="Email" value={result.customerEmail} />
          </div>

          {/* Seats */}
          <div className="card p-5">
            <h3 className="font-semibold text-sm text-gray-500 dark:text-gray-400 uppercase tracking-wide mb-3 flex items-center gap-2">
              <CreditCard className="h-4 w-4" /> Seats ({result.seats.length})
            </h3>
            <div className="space-y-1">
              {result.seats.map((seat, i) => (
                <div key={i} className="flex justify-between text-sm py-1 border-b border-gray-100 dark:border-gray-700 last:border-0">
                  <span className="font-mono font-semibold">{seat.row}{seat.number}</span>
                  <span className="text-gray-500">{seat.category}</span>
                  <span>{formatCurrency(seat.price)}</span>
                </div>
              ))}
              <div className="flex justify-between text-sm font-bold pt-2">
                <span>Total</span>
                <span className="text-primary-600 dark:text-primary-400">{formatCurrency(result.totalAmount)}</span>
              </div>
            </div>
          </div>

          {/* Scan another */}
          <button onClick={handleReset} className="btn-secondary w-full flex items-center justify-center gap-2">
            <RotateCcw className="h-4 w-4" /> Scan Another Ticket
          </button>
        </div>
      )}
    </div>
  );
}
