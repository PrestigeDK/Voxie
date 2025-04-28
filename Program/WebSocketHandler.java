package com.example.voxie.handler;

import org.springframework.stereotype.Component;
import org.springframework.web.socket.*;
import org.springframework.web.socket.handler.TextWebSocketHandler;

import java.util.concurrent.ConcurrentHashMap;

@Component
public class WebSocketHandler extends TextWebSocketHandler {

    private final ConcurrentHashMap<String, WebSocketSession> sessions = new ConcurrentHashMap<>();

    @Override
    public void afterConnectionEstablished(WebSocketSession session) throws Exception {
        String connectionId = session.getId();
        sessions.put(connectionId, session);
        System.out.println("Connection established: " + connectionId);
    }

    @Override
    protected void handleTextMessage(WebSocketSession session, TextMessage message) throws Exception {
        String connectionId = session.getId();
        System.out.println("Received: " + message.getPayload() + " from " + connectionId);

        // Broadcast the message to all connected clients
        for (WebSocketSession s : sessions.values()) {
            if (s.isOpen()) {
                s.sendMessage(new TextMessage("[" + connectionId + "]: " + message.getPayload()));
            }
        }
    }

    @Override
    public void afterConnectionClosed(WebSocketSession session, CloseStatus status) throws Exception {
        String connectionId = session.getId();
        sessions.remove(connectionId);
        System.out.println("Connection closed: " + connectionId);
    }
}
