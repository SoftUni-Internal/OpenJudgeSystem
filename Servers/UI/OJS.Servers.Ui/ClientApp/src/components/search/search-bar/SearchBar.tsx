import { ChangeEvent, useCallback, useEffect, useRef, useState } from 'react';
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
    const isNavigating = useRef(false);
    const hasInitialized = useRef(false);

    const { searchValue, selectedTerms, isVisible } = useAppSelector((state) => state.search);

    const textColorClassName = getColorClassName(themeColors.textColor);
    const backgroundColorClassName = getColorClassName(isDarkMode
        ? themeColors.baseColor200
        : themeColors.baseColor100);

    // Sync url params with UI
    useEffect(() => {
        const params = new URLSearchParams(location.search);
        const urlSearchTerm = params.get('searchTerm') || '';
        const urlSelectedTerms = CHECKBOXES.filter((term) => params.get(term) === 'true');
        const urlVisible = params.get('isVisible') === 'true';

        if (!hasInitialized.current) {
            // First time load: set all states
            setInputValue(urlSearchTerm);
            dispatch(setSearchValue(urlSearchTerm));
            hasInitialized.current = true;
        } else if (inputValue !== urlSearchTerm) {
            // After that: update state only if it has changed
            setInputValue(urlSearchTerm);
            dispatch(setSearchValue(urlSearchTerm));
        }

        dispatch(setSelectedTerms(urlSelectedTerms.length > 0 || !location.pathname.includes('/search')
            ? urlSelectedTerms
            : []));
        dispatch(setIsVisible(urlVisible));
    }, [ location.search, dispatch, location.pathname ]);

    const updateSearchParams = useCallback(
        (
            newSearchValue?: string,
            terms?: CheckboxSearchValues[],
            newIsVisibleValue?: boolean,
        ) => {
            if (isNavigating.current || !location.pathname.includes('/search')) {
                return;
            }

            const trimmedSearchValue = newSearchValue
                ? newSearchValue.trim()
                : searchValue?.trim();
            const selected = terms ?? selectedTerms;

            const params = new URLSearchParams();
            params.set('searchTerm', trimmedSearchValue || '');
            selected.forEach((term) => params.set(term, 'true'));
            params.set('isVisible', String(newIsVisibleValue !== undefined
                ? newIsVisibleValue
                : isVisible));

            const newParamsString = params.toString();
            const currentParamsString = location.search.slice(1);

            if (newParamsString !== currentParamsString) {
                setSearchParams(params);
            }
        },
        [ location.pathname, location.search, searchValue, selectedTerms, isVisible, setSearchParams ],
    );

    // Focus input when the search bar becomes visible
    useEffect(() => {
        if (isVisible) {
            document.getElementById('search-for-text-input')?.focus();
        }
    }, [ isVisible ]);

    // Reset state when leaving search page
    useEffect(() => {
        if (!location.pathname.includes('/search')) {
            setInputValue('');
            dispatch(setIsVisible(false));
            dispatch(setSearchValue(''));
            dispatch(setSelectedTerms(CHECKBOXES));
        }
    }, [ location.pathname, dispatch ]);

    // No matter what was typed, when 'Enter' is clicked we will be navigated to '/search'
    const handleSubmit = () => {
        const trimmedSearchValue = searchValue?.trim() || '';
        isNavigating.current = true;
        navigate({
            pathname: '/search',
            search: new URLSearchParams({
                searchTerm: trimmedSearchValue,
                ...Object.fromEntries(selectedTerms.map((term) => [ term, 'true' ])),
                isVisible: String(isVisible),
            }).toString(),
        });
        setTimeout(() => { isNavigating.current = false; }, 0);
    };

    const handleSearchCheckboxClick = (checkbox: string) => {
        const newSelectedTerms = selectedTerms.includes(checkbox as CheckboxSearchValues)
            ? selectedTerms.filter((term) => term !== checkbox)
            : [ ...selectedTerms, checkbox as CheckboxSearchValues ];

        dispatch(setSelectedTerms(newSelectedTerms));
        updateSearchParams(undefined, newSelectedTerms);
    };

    const debouncedDispatch = debounce((value: string) => {
        dispatch(setSearchValue(value));
        updateSearchParams(value);
    }, 250);

    const handleSearchInputChange = (e: ChangeEvent<HTMLInputElement>) => {
        const newValue = e.target.value;
        setInputValue(newValue);
        debouncedDispatch(newValue);
    };

    const handleSearchBarClose = () => {
        const newValue = !isVisible;
        dispatch(setIsVisible(newValue));
        updateSearchParams(undefined, undefined, newValue);
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
                  autoComplete="off"
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
              onClick={handleSearchBarClose}
              className={concatClassNames(styles.closeIcon, getColorClassName(themeColors.textColor))}
            />
        </div>
    );
};

export default SearchBar;
