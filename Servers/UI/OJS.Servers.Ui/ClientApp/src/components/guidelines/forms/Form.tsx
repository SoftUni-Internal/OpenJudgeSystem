import React, { FormEvent, useCallback, useMemo } from 'react';

import concatClassNames from '../../../utils/class-names';
import generateId from '../../../utils/id-generator';
import { IHaveOptionalChildrenProps, IHaveOptionalClassName } from '../../common/Props';
import { Button, ButtonType } from '../buttons/Button';

interface IFormProps extends IHaveOptionalChildrenProps, IHaveOptionalClassName {
    onSubmit: () => void;
    submitText?: string;
    id?: string;
    submitButtonClassName?: string;
    disableButton?: boolean;
    hideFormButton?: boolean;
}

const Form = ({
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
            disableButton === false
                ? (
                    <Button
                      id={btnId}
                      onClick={(ev) => handleSubmit(ev)}
                      text={submitText}
                      type={ButtonType.submit}
                      className={internalSubmitButtonClassName}
                    />
                )
                : null
        ),
        [ btnId, handleSubmit, disableButton, internalSubmitButtonClassName, submitText ],
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
