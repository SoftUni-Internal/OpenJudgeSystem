import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import { ChatMessageRole } from '../../common/enums';
import { IMentorConversationMessage } from '../../common/types';

// Maximum number of messages to store per problem
// This helps prevent storage issues while still maintaining a good conversation history
const MAX_MESSAGES_PER_PROBLEM = 50;

interface IMentorState {
    // Store conversations by problemId
    conversationsByProblemId: Record<number, {
    messages: IMentorConversationMessage[];
    conversationDate: Date | null;
    }>;
}

const initialState: IMentorState = {
    conversationsByProblemId: {},
};

export const mentorSlice = createSlice({
    name: 'mentor',
    initialState,
    reducers: {
        // Initialize a conversation for a problem if it doesn't exist
        initializeConversation: (state, action: PayloadAction<{ problemId: number; problemName: string }>) => {
            const { problemId, problemName } = action.payload;

            // Only initialize if it doesn't already exist
            if (!state.conversationsByProblemId[problemId]) {
                state.conversationsByProblemId[problemId] = {
                    messages: [
                        {
                          content: `Здравейте, аз съм Вашият ментор за писане на код, как мога да Ви помогна със задача ${problemName}?`,
                          role: ChatMessageRole.Assistant,
                          sequenceNumber: 1,
                          problemId,
                        },
                    ],
                    conversationDate: null,
                    };
                }
            },

        // Add a message to a specific problem's conversation
        addMessage: (state, action: PayloadAction<{
            problemId: number;
            message: IMentorConversationMessage;
            setConversationDate?: boolean;
        }>) => {
            const { problemId, message, setConversationDate } = action.payload;

            // Ensure the conversation exists
            if (!state.conversationsByProblemId[problemId]) {
                state.conversationsByProblemId[problemId] = {
                    messages: [],
                    conversationDate: null,
                };
            }

            // Add the message
            state.conversationsByProblemId[problemId].messages.push(message);

            // Apply message limit if exceeded
            if (state.conversationsByProblemId[problemId].messages.length > MAX_MESSAGES_PER_PROBLEM) {
                // Keep the first message (welcome message) and the most recent messages
                const welcomeMessage = state.conversationsByProblemId[problemId].messages[0];
                const recentMessages = state.conversationsByProblemId[problemId].messages.slice(-(MAX_MESSAGES_PER_PROBLEM - 1));

                // Reassign the messages array with the welcome message and recent messages
                state.conversationsByProblemId[problemId].messages = [welcomeMessage, ...recentMessages];

                // Fix sequence numbers to be consecutive
                state.conversationsByProblemId[problemId].messages.forEach((msg, index) => {
                    if (index > 0) { // Skip the welcome message
                        msg.sequenceNumber = index + 1;
                    }
                });
            }

            // Set conversation date if needed
            if (setConversationDate && !state.conversationsByProblemId[problemId].conversationDate) {
                state.conversationsByProblemId[problemId].conversationDate = new Date();
            }
        },

        // Update all messages for a problem (used when receiving response from API)
        updateMessages: (state, action: PayloadAction<{
            problemId: number;
            messages: IMentorConversationMessage[];
        }>) => {
            const { problemId, messages } = action.payload;

            // Ensure the conversation exists
            if (!state.conversationsByProblemId[problemId]) {
                state.conversationsByProblemId[problemId] = {
                    messages: [],
                    conversationDate: new Date(),
              };
            }

            // Filter out system messages AND ensure all messages have the correct problemId
            const filteredMessages = messages
                .filter(m => m.role !== ChatMessageRole.System)
                .map(m => ({
                    ...m,
                    problemId,
                }));

            // Apply message limit if exceeded
            if (filteredMessages.length > MAX_MESSAGES_PER_PROBLEM) {
                // Keep the first message (welcome message) and the most recent messages
                const welcomeMessage = filteredMessages[0];
                const recentMessages = filteredMessages.slice(-(MAX_MESSAGES_PER_PROBLEM - 1));

                // Update the messages
                state.conversationsByProblemId[problemId].messages = [
                    { ...welcomeMessage, problemId }, // Ensure welcome message has correct problemId
                    ...recentMessages.map(m => ({ ...m, problemId })) // Ensure all messages have correct problemId
                ];

                // Fix sequence numbers to be consecutive
                state.conversationsByProblemId[problemId].messages.forEach((msg, index) => {
                    if (index > 0) { // Skip the welcome message
                        msg.sequenceNumber = index + 1;
                    }
                });
            } else {
                // Update the messages (no limit exceeded)
                state.conversationsByProblemId[problemId].messages = filteredMessages;
            }
        },
    },
});

export const {
  initializeConversation,
  addMessage,
  updateMessages
} = mentorSlice.actions;

export default mentorSlice.reducer;
