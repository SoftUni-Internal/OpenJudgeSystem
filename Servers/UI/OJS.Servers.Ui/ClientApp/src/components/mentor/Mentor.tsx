import React, { useEffect, useMemo, useRef, useState } from 'react';
import { IoMdClose, IoMdSend } from 'react-icons/io';
import ReactMarkdown from 'react-markdown';
import Button from '@mui/material/Button';
import Dialog from '@mui/material/Dialog';
import DialogActions from '@mui/material/DialogActions';
import DialogContent from '@mui/material/DialogContent';
import DialogTitle from '@mui/material/DialogTitle';
import TextField from '@mui/material/TextField';
import isNil from 'lodash/isNil';
import { ChatMessageRole } from 'src/common/enums';
import { IMentorConversationMessage } from 'src/common/types';
import useTheme from 'src/hooks/use-theme';
import { addMessages } from 'src/redux/features/mentorSlice';
import { useLazyGetSystemMessageQuery, useStartConversationMutation } from 'src/redux/services/mentorService';
import { useAppDispatch, useAppSelector } from 'src/redux/store';
import concatClassNames from 'src/utils/class-names';
import { getMentorConversationDate } from 'src/utils/dates';
import { getCompositeKey } from 'src/utils/id-generator';

import mentorAvatar from '../../assets/mentor.png';

import styles from './Mentor.module.scss';

interface IMentorProps {
    problemId?: number;
    problemName?: string;
    contestId?: number;
    contestName?: string;
    categoryName?: string;
    submissionTypeName?: string;
    isMentorAllowed: boolean;
}

const Mentor = (props: IMentorProps) => {
    const { problemId, problemName, contestId, contestName, categoryName, submissionTypeName, isMentorAllowed } = props;
    const { internalUser: user } = useAppSelector((state) => state.authorization);
    const { isDarkMode } = useTheme();
    const dispatch = useAppDispatch();

    const [ isOpen, setIsOpen ] = useState(false);
    const [ showBubble, setShowBubble ] = useState(true);
    const [ inputMessage, setInputMessage ] = useState('');

    const conversationData = useAppSelector((state) => problemId && user?.id
        ? state.mentor?.conversationsByProblemId?.[getCompositeKey(user.id, problemId)]
        : undefined);

    // Local state for UI purposes
    const [ localConversationMessages, setLocalConversationMessages ] = useState<IMentorConversationMessage[]>([]);
    const [ localConversationDate, setLocalConversationDate ] = useState<Date | null>(null);

    const inputRef = useRef<HTMLInputElement>(null);
    const messagesEndRef = useRef<HTMLDivElement>(null);

    const [ startConversation, {
        data: conversationResponseData,
        error,
        isLoading,
    } ] = useStartConversationMutation({ fixedCacheKey: `problem-${problemId}` });

    const [ getSystemMessage, { isLoading: isLoadingSystemMessage } ] = useLazyGetSystemMessageQuery();

    const isInputLengthExceeded = useMemo(
        () => inputMessage.length > (conversationResponseData?.maxUserInputLength ?? 4096),
        [ conversationResponseData, inputMessage ],
    );

    const welcomeMessage: IMentorConversationMessage = useMemo(() => ({
        content: `Здравейте, аз съм Вашият ментор за писане на код, как мога да Ви помогна със задача ${problemName}?`,
        role: ChatMessageRole.Assistant,
        sequenceNumber: 1,
    }), [ problemName ]);

    const isChatDisabled = useMemo(
        () => inputMessage.trim() === '' ||
            isLoading ||
            isInputLengthExceeded ||
            problemId === undefined ||
            problemName === undefined ||
            contestId === undefined ||
            contestName === undefined ||
            categoryName === undefined ||
            submissionTypeName === undefined,
        [
            categoryName,
            contestId,
            contestName,
            inputMessage,
            isLoading,
            problemId,
            problemName,
            isInputLengthExceeded,
            submissionTypeName,
        ],
    );

    useEffect(() => {
        if (conversationData && problemId !== undefined) {
            setLocalConversationMessages(conversationData.messages);
            setLocalConversationDate(conversationData.conversationDate);
        } else if (problemId !== undefined) {
            setLocalConversationMessages([ welcomeMessage ]);
            setLocalConversationDate(null);
        }
    }, [ conversationData, problemId, welcomeMessage ]);

    useEffect(() => {
        if (messagesEndRef.current) {
            messagesEndRef.current.scrollIntoView({ behavior: 'smooth' });
        }
    }, [ localConversationMessages ]);

    useEffect(() => {
        const timer = setTimeout(() => {
            if (!isOpen) {
                setShowBubble(false);
            }
        }, 15000);

        return () => clearTimeout(timer);
    }, [ isOpen ]);

    const handleSendMessage = () => {
        if (inputMessage.trim() === '') {
            return;
        }

        if (problemId === undefined ||
            problemName === undefined ||
            contestId === undefined ||
            contestName === undefined ||
            categoryName === undefined ||
            submissionTypeName === undefined) {
            return;
        }

        const message: IMentorConversationMessage = {
            content: inputMessage,
            role: ChatMessageRole.User,
            sequenceNumber: Math.max(...localConversationMessages.map((cm) => cm.sequenceNumber)) + 1,
        };

        const messages = [ message ];

        // Add the welcome message to the store if it's the first message
        if (isNil(conversationData) || conversationData.messages.length === 0) {
            messages.unshift(welcomeMessage);
        }

        dispatch(addMessages({
            userId: user.id,
            problemId,
            messages,
            setConversationDate: localConversationDate === null,
        }));

        // Update local state for immediate UI feedback
        const updatedConversationMessages = [ ...localConversationMessages, message ];
        setLocalConversationMessages(updatedConversationMessages);
        if (localConversationDate === null) {
            setLocalConversationDate(new Date());
        }

        startConversation({
            userId: user.id,
            messages: updatedConversationMessages,
            problemId,
            problemName,
            contestId,
            contestName,
            categoryName,
            submissionTypeName,
        });

        setInputMessage('');
        inputRef.current?.focus();
    };

    const handleKeyPress = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            if (!isChatDisabled) {
                handleSendMessage();
            }
        }
    };

    const handleToggleChat = () => {
        setIsOpen(!isOpen);
        setShowBubble(false);
    };

    const handleCheckSystemMessage = async () => {
        if (problemId && problemName && contestId && contestName && categoryName && submissionTypeName &&
            !localConversationMessages.some((m) => m.role === ChatMessageRole.System)) {
            const { data } = await getSystemMessage({
                userId: user?.id || '',
                problemId,
                problemName,
                contestId,
                contestName,
                categoryName,
                submissionTypeName,
            });

            if (data) {
                setLocalConversationMessages([ ...localConversationMessages, data ]);
            }
        }
    };

    if (!isMentorAllowed) {
         
        return <></>;
    }

    return (
        <div className={styles.mentor} aria-hidden={false}>
            {showBubble && !isOpen && 
                <div className={styles.bubbleMessage}>
                    <div className={styles.primaryText}>The Code Wizard</div>
                    <div className={styles.secondaryText}>is here to help!</div>
                </div>
            }
            <Button
              variant="contained"
              className={styles.mentorButton}
              onClick={handleToggleChat}
            >
                <img src={mentorAvatar} alt="Mentor Avatar" />
            </Button>
            <Dialog
              open={isOpen}
              maxWidth="sm"
              fullWidth
              classes={{
                  paper: styles.dialogPaper,
                  root: styles.dialogRoot,
              }}
              PaperProps={{
                  style: {
                      position: 'fixed',
                      bottom: '96px',
                      right: '24px',
                      margin: 0,
                  },
              }}
              hideBackdrop
              disablePortal
              keepMounted
              disableEnforceFocus
              disableAutoFocus
            >
                <DialogTitle className={styles.dialogTitle}>
                    <Button
                      onClick={handleToggleChat}
                      className={styles.closeButton}
                    >
                        <IoMdClose />
                    </Button>
                    <div className={styles.mentorTitleContainer}>
                        <div className={styles.mentorTitleAvatar}>
                            <img src={mentorAvatar} alt="Mentor Avatar" />
                        </div>
                        <div className={styles.titleTextContainer}>
                            <span className={styles.mentorTitleText}>The Code Wizard</span>
                            {problemName && 
                                <span className={styles.problemNameText}>{problemName}</span>
                            }
                        </div>
                        {user?.isAdmin && 
                            <Button
                              onClick={handleCheckSystemMessage}
                              className={styles.systemMessageButton}
                              disabled={isLoadingSystemMessage}
                            >
                                {isLoadingSystemMessage
                                    ? 'Loading...'
                                    : 'Check System Message'}
                            </Button>
                        }
                    </div>
                </DialogTitle>

                <DialogContent
                  className={concatClassNames(
                      styles.dialogContent,
                      isDarkMode
                          ? styles.darkDialogContent
                          : styles.lightDialogContent,
                  )}
                >
                    <div className={styles.messagesContainer}>
                        <div className={styles.conversationStartDate}>
                            {localConversationDate !== null && getMentorConversationDate(localConversationDate)}
                        </div>
                        {localConversationMessages.map((message) => 
                            <div
                              className={`${styles.messageContainer} ${
                                  message.role === ChatMessageRole.System
                                      ? styles.systemMessageContainer
                                      : ''
                              }`}
                              key={message.sequenceNumber}
                            >
                                {(message.role === ChatMessageRole.Assistant ||
                                  message.role === ChatMessageRole.Information) && 
                                  <div className={styles.mentorMessageAvatar}>
                                      <img src={mentorAvatar} alt="Mentor Avatar" />
                                  </div>
                                }
                                <div
                                  className={`${styles.message} ${
                                      message.role === ChatMessageRole.User
                                          ? styles.userMessage
                                          : message.role === ChatMessageRole.System
                                              ? styles.systemMessage
                                              : styles.mentorMessage
                                  }`}
                                >
                                    <div className={styles.markdownContent}>
                                        <ReactMarkdown>{message.content}</ReactMarkdown>
                                    </div>
                                </div>
                            </div>)}
                        {isLoading && 
                            <div className={styles.message}>
                                <div className={styles.typingIndicator}>
                                    <span className={concatClassNames(styles.dot, isDarkMode
                                        ? styles.darkDot
                                        : styles.lightDot)}
                                    />
                                    <span className={concatClassNames(styles.dot, isDarkMode
                                        ? styles.darkDot
                                        : styles.lightDot)}
                                    />
                                    <span className={concatClassNames(styles.dot, isDarkMode
                                        ? styles.darkDot
                                        : styles.lightDot)}
                                    />
                                </div>
                            </div>
                        }
                        <div ref={messagesEndRef} />
                        {' '}
                    </div>
                    {error && 
                        <div className={styles.errorMessage}>
                            {((error as any)?.data?.detail ?? 'Failed to send the message. Please try again.')}
                        </div>
                    }
                </DialogContent>
                <DialogActions className={styles.dialogActions}>
                    <TextField
                      fullWidth
                      multiline
                      maxRows={4}
                      inputRef={inputRef}
                      value={inputMessage}
                      onChange={(e) => setInputMessage(e.target.value)}
                      onKeyDown={handleKeyPress}
                      placeholder="Напишете вашето съобщение..."
                      variant="standard"
                      size="small"
                      className={styles.typingField}
                    />
                    <div className={styles.sendButtonContainer}>
                        <Button
                          onClick={handleSendMessage}
                          disabled={isChatDisabled}
                        >
                            <IoMdSend
                              className={
                                    isChatDisabled
                                        ? styles.sendIconDisabled
                                        : styles.sendIconActive
                                }
                            />
                        </Button>
                        {isInputLengthExceeded && 
                            <div className={concatClassNames(styles.errorBubble, styles.bubbleMessage)}>
                                <div className={styles.secondaryText}>
                                    {`Your message exceeds the ${conversationResponseData?.maxUserInputLength
                                        ? `${conversationResponseData.maxUserInputLength}-`
                                        : ''}character limit. Please shorten it.`}
                                </div>
                            </div>
                        }
                    </div>
                </DialogActions>
            </Dialog>
        </div>
    );
};

export default Mentor;
