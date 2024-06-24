/* eslint-disable max-len */
/* eslint-disable no-useless-return */
import React, { useCallback, useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
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
import cacheService from '../../redux/cacheService';
import { setInternalUser, setIsGetUserInfoCompleted, setIsLoggedIn } from '../../redux/features/authorizationSlice';
import { useGetUserinfoQuery, useLoginMutation } from '../../redux/services/authorizationService';
import { useAppDispatch, useAppSelector } from '../../redux/store';
import concatClassNames from '../../utils/class-names';
import { getErrorMessage } from '../../utils/http-utils';
import { flexCenterObjectStyles } from '../../utils/object-utils';
import { getPlatformForgottenPasswordUrl } from '../../utils/urls';
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
    const [ shouldConfirmContinue, setShouldConfirmContinue ] = useState(false);
    const [ hasClickedContinueButton, setHasClickedContinueButton ] = useState(false);

    const navigate = useNavigate();
    const { isLoggedIn } = useAppSelector((state) => state.authorization);
    const location = useLocation();
    const dispatch = useAppDispatch();
    const { resetCache } = cacheService();
    const usernameFieldName = 'Username';
    const passwordFieldName = 'Password';

    const { data, isSuccess: isGetInfoSuccessful, refetch: refetchGetUserInfo } = useGetUserinfoQuery(null);
    const [ login, { data: loginData, isLoading, isSuccess, error } ] = useLoginMutation();

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
        if (isSuccess && !isNil(loginData) && !hasClickedContinueButton) {
            setShouldConfirmContinue(true);
            setLoginErrorMessage(loginData);
            return;
        }

        if (isSuccess && (isNil(loginData) || (shouldConfirmContinue && hasClickedContinueButton))) {
            refetchGetUserInfo();
            const returnUrl = location.state !== null
                ? `${location.state?.from?.pathname}${location.state?.from?.search}`
                : '/';
            resetCache();
            navigate(returnUrl);

            return;
        }

        if (error) {
            setLoginErrorMessage(getErrorMessage(error));
        }
    }, [ isSuccess, error, refetchGetUserInfo, location.state, navigate, loginData,
        hasClickedContinueButton, shouldConfirmContinue, resetCache ]);

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
        if (shouldConfirmContinue) {
            setHasClickedContinueButton(true);
            return;
        }

        setHasPressedLoginBtn(true);

        if (isEmpty(userName)) {
            handleOnChangeUpdateUsername('');
            return;
        }

        if (isEmpty(password)) {
            handleOnChangeUpdatePassword('');
            return;
        }

        if (!isEmpty(usernameFormError) || !isEmpty(passwordFormError)) {
            return;
        }

        login({ userName, password, rememberMe });
    };

    const renderLoginErrorMessage = useCallback(
        () => (!isNil(loginErrorMessage)
            ? (
                <span className={shouldConfirmContinue
                    ? styles.warningMessage
                    : styles.errorMessage}
                >
                    {loginErrorMessage}
                </span>
            )
            : null),
        [ shouldConfirmContinue, loginErrorMessage ],
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
                  submitText={shouldConfirmContinue
                      ? 'Continue'
                      : 'Login'}
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
                              to={getPlatformForgottenPasswordUrl()}
                              isToExternal
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
