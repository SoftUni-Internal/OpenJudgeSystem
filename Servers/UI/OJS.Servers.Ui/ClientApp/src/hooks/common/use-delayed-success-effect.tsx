import { useEffect } from 'react';

import { IDelayedSuccessEffectParams } from '../../common/types';

const useDelayedSuccessEffect = ({ isSuccess, onSuccess } : IDelayedSuccessEffectParams) => {
    /*
        We want to delay the execution of the 'onSuccess' function,
        because when we call 'onSuccess()' the pop-up will be closed.
        Closing the pop-up window right after the 'Create' button
        has been clicked is a bad user experience, that is why
        we want to wait half a second and then close the pop-up form.
     */
    useEffect(() => {
        let timeoutId: number | null = null;

        if (isSuccess && onSuccess) {
            timeoutId = window.setTimeout(() => {
                onSuccess();
            }, 500);
        }

        return () => {
            if (timeoutId) {
                clearTimeout(timeoutId);
            }
        };
    }, [ isSuccess, onSuccess ]);
};

export default useDelayedSuccessEffect;
