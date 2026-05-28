import { useEffect, useRef, useState } from 'react';
import { Html5Qrcode, Html5QrcodeSupportedFormats } from 'html5-qrcode';
import { X, Camera, CameraOff } from 'lucide-react';

interface Props {
  onScan: (data: string) => void;
  onClose: () => void;
}

export function QrCameraScanner({ onScan, onClose }: Props) {
  const containerId = 'qr-camera-container';
  const scannerRef = useRef<Html5Qrcode | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isStarting, setIsStarting] = useState(true);
  const hasScanned = useRef(false);

  useEffect(() => {
    const scanner = new Html5Qrcode(containerId, {
      formatsToSupport: [Html5QrcodeSupportedFormats.QR_CODE],
      verbose: false,
    });
    scannerRef.current = scanner;

    scanner
      .start(
        { facingMode: 'environment' }, // rear camera; falls back to front on desktop
        { fps: 10, qrbox: { width: 260, height: 260 } },
        (decodedText) => {
          if (hasScanned.current) return;
          hasScanned.current = true;
          // Stop camera before firing callback so UI doesn't flicker
          scanner.stop().catch(() => {}).finally(() => onScan(decodedText));
        },
        () => { /* frame-level errors (no QR in frame) — ignore */ },
      )
      .then(() => setIsStarting(false))
      .catch((err: unknown) => {
        const msg = err instanceof Error ? err.message : String(err);
        if (msg.toLowerCase().includes('permission')) {
          setError('Camera permission denied. Please allow camera access in your browser settings and try again.');
        } else if (msg.toLowerCase().includes('not found') || msg.toLowerCase().includes('no camera')) {
          setError('No camera found on this device.');
        } else {
          setError(`Camera error: ${msg}`);
        }
        setIsStarting(false);
      });

    return () => {
      scanner.stop().catch(() => {});
    };
  }, []); // eslint-disable-line react-hooks/exhaustive-deps

  const handleClose = () => {
    scannerRef.current?.stop().catch(() => {}).finally(onClose);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/80 p-4">
      <div className="bg-white dark:bg-gray-900 rounded-2xl w-full max-w-sm overflow-hidden shadow-2xl">
        {/* Header */}
        <div className="flex items-center justify-between px-4 py-3 border-b border-gray-200 dark:border-gray-700">
          <div className="flex items-center gap-2">
            <Camera className="h-5 w-5 text-primary-500" />
            <span className="font-semibold text-sm">Scan Ticket QR Code</span>
          </div>
          <button
            onClick={handleClose}
            className="p-1 rounded-lg hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        {/* Camera viewport */}
        <div className="relative bg-black">
          {/* html5-qrcode renders its video into this div */}
          <div id={containerId} className="w-full" />

          {/* Loading overlay */}
          {isStarting && !error && (
            <div className="absolute inset-0 flex flex-col items-center justify-center bg-black/60 gap-3">
              <div className="animate-spin rounded-full h-10 w-10 border-t-2 border-b-2 border-primary-400" />
              <p className="text-white text-sm">Starting camera...</p>
            </div>
          )}

          {/* Scan frame guide overlay */}
          {!isStarting && !error && (
            <div className="absolute inset-0 pointer-events-none flex items-center justify-center">
              <div className="w-[260px] h-[260px] relative">
                {/* Corner markers */}
                <span className="absolute top-0 left-0 w-8 h-8 border-t-4 border-l-4 border-primary-400 rounded-tl-sm" />
                <span className="absolute top-0 right-0 w-8 h-8 border-t-4 border-r-4 border-primary-400 rounded-tr-sm" />
                <span className="absolute bottom-0 left-0 w-8 h-8 border-b-4 border-l-4 border-primary-400 rounded-bl-sm" />
                <span className="absolute bottom-0 right-0 w-8 h-8 border-b-4 border-r-4 border-primary-400 rounded-br-sm" />
              </div>
            </div>
          )}
        </div>

        {/* Error state */}
        {error && (
          <div className="px-4 py-4 flex flex-col items-center gap-3">
            <CameraOff className="h-10 w-10 text-red-400" />
            <p className="text-sm text-red-600 dark:text-red-400 text-center">{error}</p>
            <button onClick={handleClose} className="btn-secondary w-full">
              Close
            </button>
          </div>
        )}

        {/* Hint */}
        {!error && (
          <p className="text-center text-xs text-gray-500 dark:text-gray-400 px-4 py-3">
            Point camera at the QR code on the ticket — it will scan automatically
          </p>
        )}
      </div>
    </div>
  );
}
