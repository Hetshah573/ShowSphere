/// <reference types="vite/client" />

interface Window {
  google: {
    accounts: {
      id: {
        initialize: (config: { client_id: string; callback: (response: { credential: string }) => void }) => void;
        renderButton: (element: HTMLElement, config: Record<string, string>) => void;
      };
    };
  };
  Razorpay: new (options: RazorpayOptions) => RazorpayInstance;
}

interface RazorpayOptions {
  key: string;
  amount: number;
  currency: string;
  name: string;
  description: string;
  order_id: string;
  prefill?: { name?: string; email?: string };
  theme?: { color?: string };
  handler: (response: RazorpayResponse) => void;
  modal?: { ondismiss?: () => void };
}

interface RazorpayInstance {
  open: () => void;
  close: () => void;
}

interface RazorpayResponse {
  razorpay_order_id: string;
  razorpay_payment_id: string;
  razorpay_signature: string;
}

interface ImportMetaEnv {
  readonly VITE_API_URL: string;
  readonly VITE_SIGNALR_URL: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}
