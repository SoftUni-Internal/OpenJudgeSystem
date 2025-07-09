interface IClearSuccessMessagesParams {
    setParentSuccessMessage?: Function;
    setSuccessMessage?: Function;
}

const clearSuccessMessages = ({
    setParentSuccessMessage,
    setSuccessMessage,
} : IClearSuccessMessagesParams) => {
    if (setSuccessMessage) {
        setSuccessMessage();
    }

    if (setParentSuccessMessage) {
        setParentSuccessMessage();
    }
};

export default clearSuccessMessages;
