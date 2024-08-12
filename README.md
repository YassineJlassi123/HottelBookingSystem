# Hotel Booking System

Welcome to the **Hotel Booking System** project! 🎉 This application provides a dynamic pricing system for hotel rooms, taking into account various factors such as room type, special requests, seasonality, and competitor pricing.

## Overview

The project is designed to handle:

### 🏷️ Dynamic Room Pricing
- **Functionality:** Adjusts prices based on room type, seasonality, and competitor data.
- **Constraints:** Ensures prices are competitive but within defined limits.

### 🛏️ Room Allocation
- **Functionality:** Allocates rooms based on availability and special requests (e.g., connecting rooms, preferred views).
- **Constraints:** Matches special requests as closely as possible.

### 🔒 Thread Safety
- **Functionality:** Manages concurrent access to shared resources like room availability.
- **Approach:** Uses locking mechanisms to ensure safe access and prevent conflicts in a multi-threaded environment.

## Key Features

### 📊 Dynamic Pricing Calculation
- **Description:** Prices are adjusted based on the base price, seasonality, occupancy rate, and competitor prices.
- **Constraints:** Ensures that prices remain competitive but within defined limits.

### 🛌 Room Allocation
- **Description:** Rooms are allocated based on availability and special requests, such as connecting rooms or preferred views.
- **Constraints:** Special requests are matched as closely as possible.

### 🔒 Thread-Safe Operations
- **Description:** Uses locking mechanisms to ensure safe access to shared resources, preventing issues in a multi-threaded environment.

## Approach to Difficult Problems

### 🧩 Handling Multi-Threaded Room Availability
- **Problem:** Ensuring thread-safe access to room availability to prevent conflicts and incorrect allocations.
- **Solution:** Implemented locking mechanisms using `lock` statements to synchronize access. This approach ensures that only one thread can modify or check room availability at a time, preventing data corruption and ensuring consistency.

### 📈 Adjusting Prices Based on Competitor Data
- **Problem:** Calculating adjusted prices by comparing with competitor pricing while considering various factors.
- **Solution:** Developed a `PricingService` that computes adjusted prices based on base prices, seasonality, occupancy rates, and competitor pricing, ensuring reasonable adjustments within defined limits.

### 🧪 Unit Testing 
- **Problem:** Testing services and controllers while isolating dependencies.

## Problems I Faced 
I have encountered many problems, and I will break them down along with how I resolved each one.First Problem: Handling CSV FilesThe first issue I faced was with processing a CSV file. When trying to retrieve records, the values weren't being returned correctly. After debugging, I discovered that some headers and columns were being considered missing by my functions. To resolve this, I added HeaderValidated = null and MissingFieldFound = null, which finally allowed me to retrieve the records correctly from the CSV file.Second Problem: Room Allocation ServiceAnother problem occurred with the Room Allocation Service. My main objective was to create a function that automatically sets a room to "unavailable" after it is allocated, preventing it from being booked again. Ideally, a message indicating that the room is unavailable should appear, but in reality, nothing happened, and the same room could still be allocated multiple times. After researching, I found that using a lock could resolve this issue as it seemed related to thread sharing. Unfortunately, this didn’t work. So, I did further research and found that using a Singleton could help. I added the following line to my Program.cs:builder.Services.AddSingleton<RoomAllocationService>();This solved the problem, and now, the same room cannot be booked again unless its availability is reset.Third Problem: Pricing LogicThe third problem was not a functionality issue but a logical one. The issue was that the CSV file showed all hotels having the same base price as mine. To address this, I decreased my hotel booking price and added dynamic pricing factors. This helped differentiate my prices.I encountered many other problems, but through research and persistence, I was able to resolve them all.
 

## Running the Application

To run the application, follow these steps:

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/YassineJlassi123/HottelBookingSystem.git

2. **Build and Run the Application:**
   ```bash
   dotnet build
   dotnet run

3. **Access the Application:**
Open your browser and navigate to http://localhost:5137.

3. **Testing:**
   Navigate to the Test Directory:
   ```bash
   cd HotelPricingEngine.Tests

4. **Run the Tests Using the .NET CLI:**
   ```bash
   dotnet test

**Production URL:**
You can test the live application at: https://hottelbookingsystem.onrender.com

**Contact:**
If you have any questions or feedback, feel free to reach out to yassinej696@gmail.com.
