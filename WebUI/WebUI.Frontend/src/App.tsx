/**
 * Main App Component
 * 主应用组件
 */

import React from 'react';
import { RouterProvider } from 'react-router-dom';
import ThemeProvider from './components/ThemeProvider';
import router from './router';
import './App.css';

function App() {
  return (
    <ThemeProvider>
      <RouterProvider router={router} />
    </ThemeProvider>
  );
}

export default App;
