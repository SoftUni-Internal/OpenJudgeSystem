/* eslint-disable max-len */
/* eslint-disable no-useless-return */
import React, { useCallback, useEffect, useState } from 'react';
import isEmpty from 'lodash/isEmpty';
import isNil from 'lodash/isNil';

import lightSoftuniLogo from '../../assets/softuni-logo-horizontal-colored.svg';
import darkSoftuniLogo from '../../assets/softuni-logo-horizontal-white.svg';
import {
    EmptyPasswordErrorMessage,
    EmptyUsernameErrorMessage,
    PasswordLengthErrorMessage,
    UsernameFormatErrorMessage, UsernameLengthErrorMessage,
} from '../../common/constants';
import useTheme from '../../hooks/use-theme';
import { setInternalUser, setIsGetUserInfoCompleted, setIsLoggedIn } from '../../redux/features/authorizationSlice';
import { useGetUserinfoQuery, useLoginMutation } from '../../redux/services/authorizationService';
import { useAppDispatch, useAppSelector } from '../../redux/store';
import concatClassNames from '../../utils/class-names';
import { flexCenterObjectStyles } from '../../utils/object-utils';
import { LinkButton, LinkButtonType } from '../guidelines/buttons/Button';
import Form from '../guidelines/forms/Form';
import FormControl, { FormControlType, IFormControlOnChangeValueType } from '../guidelines/forms/FormControl';
import Heading, { HeadingType } from '../guidelines/headings/Heading';
import SpinningLoader from '../guidelines/spinning-loader/SpinningLoader';

import styles from './LoginForm.module.scss';

const LoginForm = () => {
    const { isDarkMode, getColorClassName, themeColors } = useTheme();
    const [ userName, setUsername ] = useState<string>('');
    const [ password, setPassword ] = useState<string>('');
    const [ rememberMe, setRememberMe ] = useState<boolean>(false);
    const [ loginErrorMessage, setLoginErrorMessage ] = useState<string>('');
    const [ usernameFormError, setUsernameFormError ] = useState('');
    const [ passwordFormError, setPasswordFormError ] = useState('');
    const [ disableLoginButton, setDisableLoginButton ] = useState(false);
    const [ hasPressedLoginBtn, setHasPressedLoginBtn ] = useState(false);

    const [ login, { isLoading, isSuccess, error } ] = useLoginMutation();
    const { data, isSuccess: isGetInfoSuccessful, refetch } = useGetUserinfoQuery(null);
    const { isLoggedIn } = useAppSelector((state) => state.authorization);
    const dispatch = useAppDispatch();
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
        if (isGetInfoSuccessful && data) {
            dispatch(setInternalUser(data));
            dispatch(setIsLoggedIn(true));
            dispatch(setIsGetUserInfoCompleted(true));
        }
    }, [ isGetInfoSuccessful, data, dispatch ]);

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
    }, [ isSuccess, error, refetch ]);

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

        login({ userName, password, rememberMe });
    };

    const renderLoginErrorMessage = useCallback(
        () => (!isNil(loginErrorMessage)
            ? <span className={styles.errorMessage}>{loginErrorMessage}</span>
            : null),
        [ loginErrorMessage ],
    );

    const formClassName = concatClassNames(
        styles.loginForm,
        isDarkMode
            ? styles.darkLoginForm
            : '',
        getColorClassName(themeColors.textColor),
    );

    return (
        <div className={styles.loginFormContentContainer}>
            <LinkButton
              to="/"
              type={LinkButtonType.image}
              altText="Softuni logo"
              className={styles.logo}
              imgSrc={isDarkMode
                  ? darkSoftuniLogo
                  : lightSoftuniLogo}
            />
            <div className={formClassName}>
                <Form
                  onSubmit={() => handleLoginClick()}
                  submitText="Login"
                  isLoading={isLoading}
                  hideFormButton={isLoading || isLoggedIn}
                  disableButton={disableLoginButton}
                >
                    <header className={styles.loginFormHeader}>
                        <Heading type={HeadingType.secondary}>Login</Heading>
                        {renderLoginErrorMessage()}
                    </header>
                    <FormControl
                      id={usernameFieldName.toLowerCase()}
                      name={usernameFieldName}
                      labelText={usernameFieldName}
                      type={FormControlType.input}
                      onChange={handleOnChangeUpdateUsername}
                      value=""
                      showPlaceholder
                      shouldDisableLabel
                    />
                    <FormControl
                      id={passwordFieldName.toLowerCase()}
                      name={passwordFieldName}
                      labelText={passwordFieldName}
                      type={FormControlType.password}
                      onChange={handleOnChangeUpdatePassword}
                      value=""
                      showPlaceholder
                      shouldDisableLabel
                    />
                    <div className={styles.loginFormControls}>
                        <FormControl
                          id="auth-password-checkbox"
                          name="RememberMe"
                          labelText="Remember me"
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
                <span className={styles.registerHeader}>
                    {'You don\'t have an account yet? '}
                    <LinkButton
                      to="/register"
                      type={LinkButtonType.plain}
                      className={styles.loginFormLink}
                    >
                        Register
                    </LinkButton>
                </span>
            </div>
        </div>
    );
};

export default LoginForm;
