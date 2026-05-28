import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function formatDate(date: string) {
  return new Date(date).toLocaleDateString('en-IN', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });
}

export function formatTime(date: string) {
  return new Date(date).toLocaleTimeString('en-IN', {
    hour: '2-digit',
    minute: '2-digit',
    hour12: true,
  });
}

export function formatCurrency(amount: number) {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
  }).format(amount);
}

export function formatDuration(minutes: number) {
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  return `${h}h ${m}m`;
}

export function getStatusColor(status: string | number) {
  const s = String(status).toLowerCase();
  switch (s) {
    case 'confirmed': case '1': return 'bg-green-500/20 text-green-400';
    case 'pending': case '0': return 'bg-yellow-500/20 text-yellow-400';
    case 'cancelled': case '2': return 'bg-red-500/20 text-red-400';
    case 'expired': case '3': return 'bg-gray-500/20 text-gray-400';
    default: return 'bg-gray-500/20 text-gray-400';
  }
}
