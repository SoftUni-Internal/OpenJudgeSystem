import { createSlice, PayloadAction } from '@reduxjs/toolkit';

import { ChatMessageRole } from '../../common/enums';
import { IMentorConversationMessage } from '../../common/types';
import { getCompositeKey } from 'src/utils/id-generator';

// Maximum number of messages to store per problem
// This helps prevent storage issues while still maintaining a good conversation history
const MAX_MESSAGES_PER_PROBLEM = 50;

interface IMentorState {
    // Store conversations by problemId
    conversationsByProblemId: Record<string, {
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
    compositeKey: string,
    createDate: boolean = false,
): void => {
    if (!state.conversationsByProblemId[compositeKey]) {
        state.conversationsByProblemId[compositeKey] = {
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
        // Add a message to a specific problem's conversation
        addMessages: (state, action: PayloadAction<{
            userId: string;
            problemId: number;
            messages: IMentorConversationMessage[];
            setConversationDate?: boolean;
        }>) => {
            const { userId, problemId, messages, setConversationDate } = action.payload;

            const compositeKey = getCompositeKey(userId, problemId);

            ensureConversationExists(state, compositeKey);

            messages.forEach((msg) => {
                state.conversationsByProblemId[compositeKey].messages.push(msg);
            });

            state.conversationsByProblemId[compositeKey].messages =
                applyMessageLimits(state.conversationsByProblemId[compositeKey].messages);

            if (setConversationDate && !state.conversationsByProblemId[compositeKey].conversationDate) {
                state.conversationsByProblemId[compositeKey].conversationDate = new Date();
            }
        },

        // Update all messages for a problem (used when receiving response from API)
        updateMessages: (state, action: PayloadAction<{
            userId: string;
            problemId: number;
            messages: IMentorConversationMessage[];
        }>) => {
            const { userId, problemId, messages } = action.payload;

            const compositeKey = getCompositeKey(userId, problemId);

            ensureConversationExists(state, compositeKey);

            // Filter out system messages
            const filteredMessages = messages
                .filter((m) => m.role !== ChatMessageRole.System);

            state.conversationsByProblemId[compositeKey].messages = applyMessageLimits(filteredMessages);
        },
    },
});

export const {
    addMessages,
    updateMessages,
} = mentorSlice.actions;

export default mentorSlice.reducer;
