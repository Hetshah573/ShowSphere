import { Outlet } from 'react-router-dom';
import { Navbar } from './Navbar';

export function Layout() {
  return (
    <div className="min-h-screen flex flex-col bg-gray-100 dark:bg-gray-900 transition-colors">
      <Navbar />
      <main className="flex-1">
        <Outlet />
      </main>
      <footer className="bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700 py-8 mt-12">
        <div className="max-w-7xl mx-auto px-4 text-center text-gray-500 dark:text-gray-400">
          <p>&copy; 2026 ShowSphere. All rights reserved.</p>
        </div>
      </footer>
    </div>
  );
}
