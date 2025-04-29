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

const initialState: IMentorState = { conversationsByProblemId: {} };

const applyMessageLimits = (messages: IMentorConversationMessage[]): IMentorConversationMessage[] => {
    if (messages.length <= MAX_MESSAGES_PER_PROBLEM) {
        return messages;
    }

    // Keep the most recent messages
    const recentMessages = messages.slice(-(MAX_MESSAGES_PER_PROBLEM - 1));

    // Fix sequence numbers to be consecutive
    recentMessages.forEach((msg, index) => {
        msg.sequenceNumber = index + 1;
    });

    return recentMessages;
};

const ensureConversationExists = (
    state: IMentorState,
    problemId: number,
    createDate: boolean = false,
): void => {
    if (!state.conversationsByProblemId[problemId]) {
        state.conversationsByProblemId[problemId] = {
            messages: [],
            conversationDate: createDate
                ? new Date()
                : null,
        };
    }
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

            ensureConversationExists(state, problemId);

            state.conversationsByProblemId[problemId].messages.push(message);

            state.conversationsByProblemId[problemId].messages =
                applyMessageLimits(state.conversationsByProblemId[problemId].messages);

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

            ensureConversationExists(state, problemId);

            // Filter out system messages AND ensure all messages have the correct problemId
            const filteredMessages = messages
                .filter((m) => m.role !== ChatMessageRole.System);

            state.conversationsByProblemId[problemId].messages = applyMessageLimits(filteredMessages);
        },
    },
});

export const {
    initializeConversation,
    addMessage,
    updateMessages,
} = mentorSlice.actions;

export default mentorSlice.reducer;
