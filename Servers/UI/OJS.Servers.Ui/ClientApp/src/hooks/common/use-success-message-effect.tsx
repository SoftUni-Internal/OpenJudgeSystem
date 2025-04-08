
import { useEffect } from 'react';

import { getAndSetSuccessfulMessages } from '../../utils/messages-utils';
import clearSuccessMessages from '../../utils/success-messages-utils';

interface ISuccessMessageData {
    message: string | undefined;
    shouldGet: boolean;
}

interface ISuccessMessageEffectParams {
    data: ISuccessMessageData[];
    setParentSuccessMessage?: Function;
    setSuccessMessage?: Function;
    clearFlags: Array<boolean>;
}

const useSuccessMessageEffect = ({
    data,
    setParentSuccessMessage,
    setSuccessMessage,
    clearFlags,
}: ISuccessMessageEffectParams) => {
    /*
    The clear flags are used to determine when the success messages
    should be cleared. If the success message is not cleared and the
    same success message is received, the component will not rerender
    and the message itself will not be displayed.
     */
    useEffect(() => {
        if (clearFlags.some((flag) => flag)) {
            clearSuccessMessages({ setParentSuccessMessage, setSuccessMessage });
        }
    }, [ clearFlags, setParentSuccessMessage, setSuccessMessage ]);

    /*
    The reason we use the 'setParentSuccessMessage' is that
    when we want to close the pop-up form right after the 'Create'
    button has been clicked, if we render the success message
    inside the 'Create' form, then after closing it, the message
    will disappear. That is why in such a case, we want to render
    the success message on the parent component using
    'setParentSuccessMessage'.
     */

    useEffect(() => {
        const messages : ISuccessMessageData[] = [];

        data.forEach((d) => d.message && messages.push({ message: d.message, shouldGet: d.shouldGet }));

        const message = getAndSetSuccessfulMessages(messages);

        if (!message) {
            return;
        }

        if (setParentSuccessMessage) {
            setParentSuccessMessage(message);
        } else if (setSuccessMessage) {
            setSuccessMessage(message);
        }
    }, [ setParentSuccessMessage, setSuccessMessage, data ]);
};

export default useSuccessMessageEffect;
