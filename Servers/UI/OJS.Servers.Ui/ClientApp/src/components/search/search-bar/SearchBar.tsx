import { useCallback, useEffect, useRef, useState } from 'react';
import { IoIosClose } from 'react-icons/io';
import { useLocation, useNavigate } from 'react-router-dom';
import { Checkbox, TextField } from '@mui/material';
import debounce from 'lodash/debounce';
import usePreserveScrollOnSearchParamsChange from 'src/hooks/common/usePreserveScrollOnSearchParamsChange';
import concatClassNames from 'src/utils/class-names';

import { CheckboxSearchValues } from '../../../common/enums';
import useTheme from '../../../hooks/use-theme';
import { setIsVisible, setSearchValue, setSelectedTerms } from '../../../redux/features/searchSlice';
import { useAppDispatch, useAppSelector } from '../../../redux/store';
import Form from '../../guidelines/forms/Form';

import styles from './SearchBar.module.scss';

const CHECKBOXES: Array<CheckboxSearchValues> = [
    CheckboxSearchValues.contests,
    CheckboxSearchValues.users,
    CheckboxSearchValues.problems,
];

const SearchBar = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const { setSearchParams } = usePreserveScrollOnSearchParamsChange();
    const dispatch = useAppDispatch();
    const { isDarkMode, themeColors, getColorClassName } = useTheme();
    const [ inputValue, setInputValue ] = useState<string>('');
    const initialMount = useRef(true);

    const { searchValue, selectedTerms, isVisible } = useAppSelector((state) => state.search);

    const textColorClassName = getColorClassName(themeColors.textColor);
    const backgroundColorClassName = getColorClassName(isDarkMode
        ? themeColors.baseColor200
        : themeColors.baseColor100);

    const updateSearchParams = useCallback(() => {
        const params = new URLSearchParams();

        if (searchValue) {
            params.set('searchTerm', searchValue);
        }

        selectedTerms.forEach((term) => params.set(term, 'true'));
        setSearchParams(params);
    }, [ searchValue, selectedTerms, setSearchParams ]);

    useEffect(() => {
        if (isVisible) {
            document.getElementById('search-for-text-input')?.focus();
        }
    }, [ isVisible ]);

    useEffect(() => {
        if (initialMount.current) {
            initialMount.current = false;
            return;
        }

        if (isVisible && searchValue?.trim().length >= 3) {
            updateSearchParams();
        }
    }, [ updateSearchParams, isVisible, searchValue, selectedTerms ]);

    useEffect(() => {
        if (!location.pathname.includes('/search')) {
            setInputValue('');
            dispatch(setIsVisible(false));
            dispatch(setSearchValue(''));
            dispatch(setSelectedTerms([
                CheckboxSearchValues.contests,
                CheckboxSearchValues.problems,
                CheckboxSearchValues.users,
            ]));
        }
    }, [ location.pathname, dispatch ]);

    const handleSubmit = () => {
        navigate({ pathname: '/search' });
    };

    const handleSearchCheckboxClick = (checkbox: string) => {
        if (selectedTerms.includes(checkbox as CheckboxSearchValues)) {
            const newSelectedItems = selectedTerms.filter((term) => term !== checkbox);
            dispatch(setSelectedTerms(newSelectedItems));
        } else {
            dispatch(setSelectedTerms([ ...selectedTerms, checkbox as CheckboxSearchValues ]));
        }
    };

    // wait user to stop typing, then dispatch in order not to dispatch on every key click
    // eslint-disable-next-line react-hooks/exhaustive-deps
    const debouncedDispatch = useCallback(debounce((value) => {
        dispatch(setSearchValue(value));
    }, 250), [ dispatch ]);

    const handleSearchInputChange = (e: any) => {
        setInputValue(e.target.value as string);
        debouncedDispatch(e.target.value as string);
    };

    return (
        <div
          className={`${styles.searchContainer} ${backgroundColorClassName} ${isVisible
              ? styles.show
              : ''}`}
        >
            <Form className={styles.search} onSubmit={handleSubmit} hideFormButton>
                <TextField
                  id="search-for-text-input"
                  variant="standard"
                  className={`${styles.searchInput} ${textColorClassName}`}
                  value={inputValue}
                  placeholder="Search value..."
                  InputLabelProps={{ shrink: true }}
                  onChange={handleSearchInputChange}
                  type="text"
                />
                <div className={styles.checkboxContainer}>
                    {CHECKBOXES.map((checkbox) => (
                        <div key={`search-bar-checkbox-${checkbox}`} className={styles.checkboxWrapper}>
                            <Checkbox
                              sx={{
                                  '&.Mui-checked': {
                                      color: '#44a9f8',
                                      '&:hover': { backgroundColor: 'transparent' },
                                  },
                              }}
                              className={styles.checkbox}
                              checked={selectedTerms.includes(checkbox)}
                              onClick={() => handleSearchCheckboxClick(checkbox)}
                            />
                            <span className={`${styles.checkboxText} ${textColorClassName}`}>
                                {checkbox}
                            </span>
                        </div>
                    ))}
                </div>
            </Form>
            <IoIosClose
              size={50}
              onClick={() => dispatch(setIsVisible(!isVisible))}
              className={concatClassNames(styles.closeIcon, getColorClassName(themeColors.textColor))}
            />
        </div>
    );
};

export default SearchBar;
