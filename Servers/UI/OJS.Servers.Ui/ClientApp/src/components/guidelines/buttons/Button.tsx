import React, { ReactNode } from 'react';
import { Link } from 'react-router-dom';
import isEmpty from 'lodash/isEmpty';
import isNil from 'lodash/isNil';

import useTheme from '../../../hooks/use-theme';
import isNilOrEmpty from '../../../utils/check-utils';
import concatClassNames from '../../../utils/class-names';
import generateId from '../../../utils/id-generator';
import { IHaveOptionalChildrenProps, IHaveOptionalClassName } from '../../common/Props';

import styles from './Button.module.scss';

enum ButtonState {
    enabled = 1,
    disabled = 2,
}
enum ButtonType {
    primary = 1,
    secondary = 2,
    plain = 3,
    image = 4,
    submit = 5,
    toggled = 6,
    untoggled = 7,
    darkNeutral = 8,
    lightNeutral = 9,
}
enum LinkButtonType {
    primary = 1,
    secondary = 2,
    plain = 3,
    image = 4,
}
enum ButtonSize {
    small = 1,
    medium = 2,
    large = 3,
    none = 4,
}
interface IButtonBaseProps<TButtonType> extends IHaveOptionalClassName, IHaveOptionalChildrenProps {
    id?: string;
    size?: ButtonSize;
    text?: string | null;
    type?: TButtonType;
    state?: ButtonState;
    imgSrc?: string;
    altText?: string;
}
interface IButtonProps extends IButtonBaseProps<ButtonType> {
    onClick: React.MouseEventHandler<HTMLButtonElement>;
    isWide?: boolean;
    internalClassName?: string;
    style?: object;
    isCompete?: boolean;
}
interface ILinkButtonProps extends IButtonBaseProps<LinkButtonType> {
    to: string;
    isToExternal?: boolean;
    internalClassName?: string;
}

const classNameToButonType = {
    [ButtonType.primary]: styles.primary,
    [ButtonType.submit]: styles.primary,
    [ButtonType.secondary]: styles.secondary,
    [ButtonType.plain]: styles.plain,
    [ButtonType.image]: styles.image,
    [ButtonType.toggled]: styles.toggled,
    [ButtonType.untoggled]: styles.untoggled,
    [ButtonType.darkNeutral]: styles.darkNeutral,
    [ButtonType.lightNeutral]: styles.lightNeutral,
};

const classNameToLinkButonType = {
    [LinkButtonType.primary]: styles.primary,
    [LinkButtonType.secondary]: styles.secondary,
    [LinkButtonType.plain]: styles.plain,
    [LinkButtonType.image]: styles.image,
};

const sizeToClassName = {
    [ButtonSize.small]: styles.small,
    [ButtonSize.medium]: styles.medium,
    [ButtonSize.large]: styles.large,
    [ButtonSize.none]: styles.none,
};

const themingButtonsClassNames:{ [key in ButtonType]?: string[] } =
    { [ButtonType.secondary]: [ styles.lightSecondary, styles.darkSecondary ] };

const validateOnlyChildrenOrText = (text: string | null, children: ReactNode | null) => {
    if (!isNil(text) && !isNil(children)) {
        throw new Error('Buttons must have only `text` or `children`');
    }
};

const Button = ({
    onClick,
    text = null,
    children = null,
    className = '',
    type = ButtonType.primary,
    size = ButtonSize.medium,
    id = generateId(),
    state = ButtonState.enabled,
    isWide = false,
    internalClassName = '',
    imgSrc = '',
    altText = '',
    isCompete = false,
    style,
}: IButtonProps) => {
    validateOnlyChildrenOrText(text, children);
    const { isDarkMode } = useTheme();

    const { [type]: typeClassName } = classNameToButonType;

    const { [size]: sizeClassName } = sizeToClassName;

    const themingClassName = themingButtonsClassNames[type]?.at(isDarkMode
        ? 1
        : 0);

    const stateClassName = state === ButtonState.disabled
        ? styles.disabled
        : '';

    const wideClassName = isWide
        ? styles.wide
        : '';

    const competeClassName = isCompete
        ? styles.competeClassName
        : '';

    const buttonClassName =
        isNilOrEmpty(internalClassName)
            ? concatClassNames(
                styles.btn,
                typeClassName,
                sizeClassName,
                stateClassName,
                wideClassName,
                competeClassName,
                themingClassName,
                className,
            )
            : internalClassName;

    const content = type === ButtonType.image
        ? <img src={imgSrc} alt={altText} />
        : children ?? text;

    return (
        <button
          type={type === ButtonType.submit
              ? 'submit'
              : 'button'}
          onClick={onClick}
          className={buttonClassName}
          id={id}
          style={style}
        >
            {content}
        </button>
    );
};

const LinkButton = ({
    to,
    text = null,
    children = null,
    className = '',
    type = LinkButtonType.primary,
    size = ButtonSize.medium,
    id = generateId(),
    state = ButtonState.enabled,
    isToExternal = false,
    imgSrc = '',
    altText = '',
    internalClassName = '',
}: ILinkButtonProps) => {
    validateOnlyChildrenOrText(text, children);
    const isDisabled = state === ButtonState.disabled;
    const toFixed = `${to.startsWith('/')
        ? ''
        : '/'}${to}`;

    const { [type]: typeClassName } = classNameToLinkButonType;

    const { [size]: sizeClassName } = sizeToClassName;

    const stateClassName = isDisabled
        ? styles.disabled
        : '';

    const buttonClassName = isEmpty(internalClassName)
        ? concatClassNames(
            styles.btn,
            typeClassName,
            sizeClassName,
            stateClassName,
            className,
        )
        : internalClassName;

    const content = type === LinkButtonType.image
        ? <img src={imgSrc} alt={altText} />
        : children ?? text;

    if (isToExternal) {
        return (
            <a
              href={toFixed}
              className={buttonClassName}
              id={id}
              target="_blank"
              rel="noopener noreferrer"
            >
                {content}
            </a>
        );
    }

    return (
        <Link
          to={toFixed}
          className={buttonClassName}
          id={id}
        >
            {content}
        </Link>
    );
};

export default Button;

export {
    Button,
    LinkButton,
    ButtonType,
    LinkButtonType,
    ButtonSize,
    ButtonState,
};
