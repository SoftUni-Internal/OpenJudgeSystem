import React, { FormEvent, useCallback, useMemo } from 'react';

import concatClassNames from '../../../utils/class-names';
import generateId from '../../../utils/id-generator';
import { IHaveOptionalChildrenProps, IHaveOptionalClassName } from '../../common/Props';
import { Button, ButtonState, ButtonType } from '../buttons/Button';
import SpinningLoader from '../spinning-loader/SpinningLoader';

interface IFormProps extends IHaveOptionalChildrenProps, IHaveOptionalClassName {
    onSubmit: () => void;
    submitText?: string;
    id?: string;
    submitButtonClassName?: string;
    disableButton?: boolean;
    hideFormButton?: boolean;
    isLoading?: boolean;
}

const Form = ({
    isLoading = false,
    onSubmit,
    children,
    submitText = 'Submit',
    id = generateId(),
    className = '',
    submitButtonClassName = '',
    disableButton = false,
    hideFormButton = false,
}: IFormProps) => {
    const handleSubmit = useCallback(
        async (ev: FormEvent) => {
            ev.preventDefault();
            onSubmit();

            return false;
        },
        [ onSubmit ],
    );

    const btnId = useMemo(
        () => `btn-submit-${id}`,
        [ id ],
    );

    const internalClassName = concatClassNames(className);
    const internalSubmitButtonClassName = concatClassNames('btnSubmitInForm', submitButtonClassName);

    const renderButton = useCallback(
        () => (
            isLoading
                ? <SpinningLoader />
                : !disableButton
                    ? (
                        <Button
                          id={btnId}
                          onClick={(ev) => handleSubmit(ev)}
                          text={submitText}
                          type={ButtonType.submit}
                          className={internalSubmitButtonClassName}
                        />
                    )
                    : (
                        <Button
                          id={btnId}
                          onClick={(ev) => handleSubmit(ev)}
                          text={submitText}
                          type={ButtonType.submit}
                          className={internalSubmitButtonClassName}
                          state={ButtonState.disabled}
                        />
                    )
        ),
        [ btnId, handleSubmit, disableButton, internalSubmitButtonClassName, submitText, isLoading ],
    );

    return (
        <form
          id={id}
          onSubmit={(ev) => handleSubmit(ev)}
          className={internalClassName}
        >
            {children}
            {!hideFormButton && renderButton()}
        </form>
    );
};

export default Form;
