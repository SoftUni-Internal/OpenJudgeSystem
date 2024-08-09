import { useEffect } from 'react';

import { ISuccessMessageData, ISuccessMessageEffectParams } from '../../common/types';
import { getAndSetSuccessfulMessages } from '../../utils/messages-utils';

const useSuccessMessageEffect = ({
    data,
    setParentSuccessMessage,
    setSuccessMessage,
}: ISuccessMessageEffectParams) => {
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

        if (data) {
            data.forEach((d) => messages.push({ message: d.message, shouldGet: d.shouldGet }));
        }

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
