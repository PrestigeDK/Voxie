# Voxie

Voxie is a real-time communication platform that enables users to connect through text chat, voice calls, and more. Built with Java and WebSocket technology, Voxie is designed to be scalable, intuitive, and feature-rich.

## Features

- **Text Chat**: Real-time messaging with support for broadcasting and private chats.
- **Voice Calls**: Peer-to-peer voice communication using WebRTC.
- **User Authentication**: Secure login and registration with JWT-based authentication.
- **Friend System**: Add and manage friends for private conversations.
- **Scalable Architecture**: Designed to grow with cloud hosting and load balancing.

## Plan of Action

### 1. Text Chat
- Implement WebSocket-based messaging.
- Broadcast messages to all connected clients or specific groups.
- Save chat history in a database for later retrieval.

### 2. Voice Calls
- Use WebRTC for peer-to-peer voice communication.
- Implement a signaling server for WebRTC offer/answer and ICE candidate exchange.
- Integrate STUN/TURN servers for NAT traversal.

### 3. User Authentication
- Add user registration and login functionality.
- Use JWT for secure authentication.
- Implement user roles and permissions.

### 4. UI/UX Design
- Create a simple and intuitive interface for chat and voice calls.
- Use a frontend framework like React or plain HTML/CSS/JavaScript.

### 5. Scalability
- Host the app on a cloud platform (e.g., AWS, Azure).
- Use SQLite initially, with plans to migrate to PostgreSQL for scalability.
- Add load balancing for WebSocket connections.

### 6. Future Features
- Video calls and screen sharing.
- Customizable themes (e.g., light/dark mode).
- Mobile app development using React Native or Flutter.

## Getting Started

### Prerequisites
- Java 17 or higher
- Maven or Gradle for dependency management
- A modern web browser for client-side testing

### Running the Application
1. Clone the repository:
   ```bash
   git clone https://github.com/PrestigeDK/Voxie.git
   cd Voxie
