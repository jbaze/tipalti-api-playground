#!/bin/bash

###############################################################################
# Development Startup Script
# Starts both backend and frontend in development mode
###############################################################################

echo "🚀 Starting Tipalti Invoice API Playground..."
echo ""

# Check if required tools are installed
command -v dotnet >/dev/null 2>&1 || { echo "❌ .NET SDK is not installed. Please install .NET 8 SDK."; exit 1; }
command -v npm >/dev/null 2>&1 || { echo "❌ npm is not installed. Please install Node.js."; exit 1; }

# Function to cleanup on exit
cleanup() {
    echo ""
    echo "🛑 Shutting down services..."
    kill $BACKEND_PID $FRONTEND_PID 2>/dev/null
    exit 0
}

trap cleanup INT TERM

# Start backend in background
echo "📦 Starting .NET backend on http://localhost:5000..."
cd backend
dotnet run &
BACKEND_PID=$!
cd ..

# Wait for backend to start
sleep 3

# Start frontend in background
echo "🎨 Starting Angular frontend on http://localhost:4200..."
cd frontend
npm start &
FRONTEND_PID=$!
cd ..

echo ""
echo "✅ Services started!"
echo ""
echo "📊 Backend API:  http://localhost:5000"
echo "🎨 Frontend App: http://localhost:4200"
echo "📚 Swagger UI:   http://localhost:5000"
echo ""
echo "Press Ctrl+C to stop all services"
echo ""

# Wait for processes
wait $BACKEND_PID $FRONTEND_PID
