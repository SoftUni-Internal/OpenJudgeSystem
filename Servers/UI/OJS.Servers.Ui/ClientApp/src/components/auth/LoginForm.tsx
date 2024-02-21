/* eslint-disable max-len */
/* eslint-disable no-useless-return */
import React, { useCallback, useEffect, useState } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import isEmpty from 'lodash/isEmpty';
import isNil from 'lodash/isNil';

import {
    EmptyPasswordErrorMessage,
    EmptyUsernameErrorMessage,
    PasswordLengthErrorMessage,
    UsernameFormatErrorMessage, UsernameLengthErrorMessage,
} from '../../common/constants';
import { IAuthorizationReduxState, setInternalUser, setIsLoggedIn } from '../../redux/features/authorizationSlice';
import { useGetUserinfoQuery, useLoginMutation } from '../../redux/services/authorizationService';
import { flexCenterObjectStyles } from '../../utils/object-utils';
import { LinkButton, LinkButtonType } from '../guidelines/buttons/Button';
import Form from '../guidelines/forms/Form';
import FormControl, { FormControlType, IFormControlOnChangeValueType } from '../guidelines/forms/FormControl';
import Heading, { HeadingType } from '../guidelines/headings/Heading';
import SpinningLoader from '../guidelines/spinning-loader/SpinningLoader';

import styles from './LoginForm.module.scss';

const LoginPage = () => {
    const [ userName, setUsername ] = useState<string>('');
    const [ password, setPassword ] = useState<string>('');
    const [ rememberMe, setRememberMe ] = useState<boolean>(false);
    const [ loginErrorMessage, setLoginErrorMessage ] = useState<string>('');
    const [ usernameFormError, setUsernameFormError ] = useState('');
    const [ passwordFormError, setPasswordFormError ] = useState('');
    const [ disableLoginButton, setDisableLoginButton ] = useState(false);
    const [ hasPressedLoginBtn, setHasPressedLoginBtn ] = useState(false);

    const [ login, { isLoading, isSuccess, error, isError } ] = useLoginMutation();
    const { data, isSuccess: isGetInfoSuccessfull, refetch } = useGetUserinfoQuery(null);
    const { isLoggedIn } =
    useSelector((state: {authorization: IAuthorizationReduxState}) => state.authorization);
    const dispatch = useDispatch();
    const usernameFieldName = 'Username';
    const passwordFieldName = 'Password';

    const handleOnChangeUpdateUsername = useCallback((value?: IFormControlOnChangeValueType) => {
        if (isEmpty(value)) {
            setUsernameFormError(EmptyUsernameErrorMessage);
        } else if (!isNil(value) && (value.length < 5 || value.length > 32)) {
            setUsernameFormError(UsernameLengthErrorMessage);
        } else {
            const regex = /^[a-zA-Z][a-zA-Z0-9._]{3,30}[a-zA-Z0-9]$/;
            if (!regex.test(value as string)) {
                setUsernameFormError(UsernameFormatErrorMessage);
            } else {
                setUsernameFormError('');
            }
        }

        setUsername(isNil(value)
            ? ''
            : value as string);
    }, [ setUsername ]);

    useEffect(() => {
        if (isGetInfoSuccessfull && data) {
            dispatch(setInternalUser(data));
            dispatch(setIsLoggedIn(true));
        }
    }, [ isGetInfoSuccessfull, data ]);

    const handleOnChangeUpdatePassword = useCallback((value?: IFormControlOnChangeValueType) => {
        if (isEmpty(value)) {
            setPasswordFormError(EmptyPasswordErrorMessage);
        } else if (!isNil(value) && value.length < 6) {
            setPasswordFormError(PasswordLengthErrorMessage);
        } else {
            setPasswordFormError('');
        }

        setPassword(isNil(value)
            ? ''
            : value as string);
    }, [ setPassword ]);

    useEffect(() => {
        if (isSuccess) {
            refetch();
            return;
        }
        if (error && 'error' in error) {
            setLoginErrorMessage(error.data as string);
        }
    }, [ isSuccess, isError ]);

    useEffect(() => {
        if (!isEmpty(usernameFormError) && hasPressedLoginBtn) {
            setLoginErrorMessage(usernameFormError);
            setDisableLoginButton(true);
            return;
        }

        if (!isEmpty(passwordFormError) && hasPressedLoginBtn) {
            setLoginErrorMessage(passwordFormError);
            setDisableLoginButton(true);
            return;
        }

        setLoginErrorMessage('');
        setDisableLoginButton(false);
    }, [ usernameFormError, passwordFormError, setLoginErrorMessage, hasPressedLoginBtn ]);

    const handleLoginClick = () => {
        /* TODO:  Add message to notify the admin if SULS is not working.
         Get the message from legacy Judge.
        */

        setHasPressedLoginBtn(true);

        if (!isEmpty(usernameFormError) || !isEmpty(passwordFormError)) {
            return;
        }

        login({ Username: userName, Password: password, RememberMe: rememberMe });
    };

    const renderLoginErrorMessage = useCallback(
        () => (!isNil(loginErrorMessage)
            ? <span className={styles.errorMessage}>{loginErrorMessage}</span>
            : null),
        [ loginErrorMessage ],
    );

    return (
        <Form
          className={styles.loginForm}
          onSubmit={() => handleLoginClick()}
          submitText="Login"
          hideFormButton={isLoading || isLoggedIn}
          disableButton={disableLoginButton}
        >
            <header className={styles.loginFormHeader}>
                <Heading type={HeadingType.primary}>Login</Heading>
                <span className={styles.registerHeader}>
                    { 'You don\'t have an account yet? '}
                    <LinkButton
                      to="/register"
                      type={LinkButtonType.plain}
                      className={styles.loginFormLink}
                    >
                        Register
                    </LinkButton>
                </span>
                { renderLoginErrorMessage() }
            </header>
            <FormControl
              id={usernameFieldName.toLowerCase()}
              name={usernameFieldName}
              labelText={usernameFieldName}
              type={FormControlType.input}
              onChange={handleOnChangeUpdateUsername}
              value=""
              showPlaceholder={false}
            />
            <FormControl
              id={passwordFieldName.toLowerCase()}
              name={passwordFieldName}
              labelText={passwordFieldName}
              type={FormControlType.password}
              onChange={handleOnChangeUpdatePassword}
              value=""
              showPlaceholder={false}
            />
            <div className={styles.loginFormControls}>
                <FormControl
                  id="auth-password-checkbox"
                  name="RememberMe"
                  labelText="Remember Me"
                  type={FormControlType.checkbox}
                  checked={rememberMe}
                  onChange={() => setRememberMe(!rememberMe)}
                />
                <div>
                    <LinkButton
                      type={LinkButtonType.plain}
                      to="/Account/ExternalNotify"
                      className={styles.loginFormLink}
                    >
                        Forgotten password
                    </LinkButton>
                </div>
            </div>
            {isLoading && (
            <div className={styles.loginFormLoader}>
                <div style={{ ...flexCenterObjectStyles }}>
                    <SpinningLoader />
                </div>
            </div>
            )}
        </Form>
    );
};

export default LoginPage;
