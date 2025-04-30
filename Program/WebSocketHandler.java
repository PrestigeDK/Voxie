package com.example.voxie.handler;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;
import org.springframework.web.socket.*;
import org.springframework.web.socket.handler.TextWebSocketHandler;

import java.util.concurrent.ConcurrentHashMap;

@Component
public class WebSocketHandler extends TextWebSocketHandler {

    private static final Logger logger = LoggerFactory.getLogger(WebSocketHandler.class);
    private final ConcurrentHashMap<String, WebSocketSession> sessions = new ConcurrentHashMap<>();

    @Override
    public void afterConnectionEstablished(WebSocketSession session) throws Exception {
        String connectionId = session.getId();
        sessions.put(connectionId, session);
        logger.info("Connection established: {}", connectionId);
    }

    @Override
    protected void handleTextMessage(WebSocketSession session, TextMessage message) throws Exception {
        String payload = message.getPayload();
        logger.info("Received message: {}", payload);

        ObjectMapper objectMapper = new ObjectMapper();
        try {
            JsonNode messageNode = objectMapper.readTree(payload);

            if (!messageNode.has("type") || !messageNode.has("data")) {
                logger.warn("Invalid message format: missing 'type' or 'data' fields");
                session.sendMessage(new TextMessage("Error: Invalid message format"));
                return;
            }

            String type = messageNode.get("type").asText();
            String data = messageNode.get("data").asText();

            switch (type) {
                case "offer":
                case "answer":
                case "ice-candidate":
                    for (WebSocketSession s : sessions.values()) {
                        if (!s.getId().equals(session.getId()) && s.isOpen()) {
                            try {
                                s.sendMessage(new TextMessage(payload));
                            } catch (Exception e) {
                                logger.error("Failed to send message to session {}: {}", s.getId(), e.getMessage());
                            }
                        }
                    }
                    break;
                default:
                    logger.warn("Unknown message type: {}", type);
                    session.sendMessage(new TextMessage("Error: Unknown message type"));
            }
        } catch (Exception e) {
            logger.error("Error processing message: {}", e.getMessage());
            session.sendMessage(new TextMessage("Error: Unable to process message"));
        }
    }

    @Override
    public void afterConnectionClosed(WebSocketSession session, CloseStatus status) throws Exception {
        String connectionId = session.getId();
        sessions.remove(connectionId);
        logger.info("Connection closed: {}", connectionId);
    }
}
