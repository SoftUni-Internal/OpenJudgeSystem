import { ThemeMode } from '../common/enums';
import { toggleTheme } from '../redux/features/themeSlice';
import { useAppDispatch, useAppSelector } from '../redux/store';

import styles from '../styles/theme-styles.module.scss';

interface ITheme {
    [key: string]: {
        textColor: string;
        baseColor100: string;
        baseColor200: string;
        baseColor300: string;
        baseColor400: string;
        baseColor500: string;
    };
}

interface IThemeClassName {
    [key: string]: string;
}

const useTheme = () => {
    const dispatch = useAppDispatch();
    const { mode } = useAppSelector((state) => state.theme);

    const isDarkMode = mode === ThemeMode.Dark;

    const toggleSelectedTheme = () => {
        dispatch(toggleTheme());
    };

    // 100: Very light; used for backgrounds and highlights.
    // 200: Light; for subtle accents or secondary backgrounds.
    // 300: Medium-light; for hover states and less prominent elements.
    // 400: Mid-tone; for borders, secondary buttons, and emphasized text.
    // 500: Main color; for primary actions and key elements.
    const themeColors: ITheme = {
        light: {
            // Using rgb() to avoid dictionary key collision
            // with dark.baseColor100 in colorClassName below
            textColor: 'rgb(104, 116, 135)', // #687487
            baseColor100: '#FFFFFF',
            baseColor200: '#E6E6E6',
            baseColor300: '#E6E6E6',
            // Pages background
            baseColor400: '#f8f8f8',
            baseColor500: '#D9D9D9',
        },
        dark: {
            textColor: '#f3f1f1',
            baseColor100: '#687487',
            baseColor200: '#374151',
            baseColor300: '#2C2F37',
            // Pages background
            baseColor400: '#212328',
            baseColor500: '#1c1c17',
        },
    };

    const colorClassName: IThemeClassName = {
        // text color class names
        [themeColors.light.textColor]: styles.darkGrayClassName,
        [themeColors.dark.textColor]: styles.whiteColorClassName,
        // dark color class names
        [themeColors.dark.baseColor100]: styles.darkBaseColor100ClassName,
        [themeColors.dark.baseColor200]: styles.darkBaseColor200ClassName,
        [themeColors.dark.baseColor300]: styles.darkBaseColor300ClassName,
        [themeColors.dark.baseColor400]: styles.darkBaseColor400ClassName,
        [themeColors.dark.baseColor500]: styles.darkBaseColor500ClassName,
        // light color class names TBD
        [themeColors.light.baseColor100]: styles.lightBaseColor100ClassName,
        [themeColors.light.baseColor200]: styles.lightBaseColor200ClassName,
        [themeColors.light.baseColor300]: styles.lightBaseColor300ClassName,
        [themeColors.light.baseColor400]: styles.lightBaseColor400ClassName,
        [themeColors.light.baseColor500]: styles.lightBaseColor500ClassName,
    };

     
    const selectedThemeColors = themeColors[mode];

    const getColorClassName = (color: string) => colorClassName[color];

    return { mode, toggleSelectedTheme, isDarkMode, themeColors: selectedThemeColors, getColorClassName };
};
export default useTheme;
