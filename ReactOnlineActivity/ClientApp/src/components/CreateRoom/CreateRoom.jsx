import React, { Component } from 'react';
import { withRouter } from 'react-router';
import authorizeFetch from '../../utils/authorizeFetch';
import styles from './createRoom.module.css';
import { Modal } from '@skbkontur/react-ui/components/Modal';
import { Button } from '@skbkontur/react-ui/components/Button';
import { Input } from '@skbkontur/react-ui/components/Input';
import { Gapped } from '@skbkontur/react-ui';

class CreateRoom extends Component {
    constructor(props) {
        super(props);

        this.state = {
            loading: false,
            modalOpened: false,
            settings: {
                name: '',
                description: '',
                roundTime: 60,
                maxPlayerCount: 5,
                minPlayerCount: 2,
                pointsToWin: 100,
                isPrivateRoom: false,
                isSelected: false,
                themes: [],
            },
            theme: {
                name: '',
                words: []
            },
            wordToAdd: ''
        };
    }

    async componentDidMount() {
        await this.fetchThemes();
    }

    async fetchThemes() {
        const themes = await authorizeFetch('/api/themes');
        this.setState({
            loading: false,
            modalOpened: false,
            settings: {
                name: '',
                description: '',
                roundTime: 60,
                maxPlayerCount: 5,
                pointsToWin: 100,
                isPrivateRoom: false,
                themes: themes || []
            },
            theme: this.state.theme,
            wordToAdd: ''
        });
    }

    renderTheme = (theme) => {
        return (
            <li onClick={() => this.onThemeClicked(theme)} className={styles.themes__theme} style={{
                backgroundImage: `url(${theme.pictureUrl})`,
                backgroundRepeat: 'no-repeat',
                backgroundSize: 'cover'
            }}>
                {this.renderThemeSquare(theme.name, theme.isSelected, styles.themes__text)}
            </li>
        );
    };

    renderThemeSquare(text, isSelected, text_style) {
        const style = isSelected ? styles.themes__selected : styles.themes__not_selected;
        return <div className={style}>
            <p className={text_style}>
                {text}
            </p>
        </div>;
    }

    onThemeClicked(theme) {
        theme.isSelected = !theme.isSelected;
        this.setState({ themes: this.state.themes });
    }

    handleSettingsChange = (fieldName, newValue) => {
        this.setState({
            settings: {
                ...this.state.settings,
                [fieldName]: newValue
            }
        });
    }

    handleSubmit = async (event) => {
        event.preventDefault();
        const newRoomId = await authorizeFetch('/api/rooms', {
            method: 'POST',
            redirect: 'manual',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(this.getRoomSettingsDto(this.state.settings))
        });
        this.props.history.push(`/rooms/${newRoomId}`);
    }

    getRoomSettingsDto(settings) {
        return {
            name: settings.name,
            description: settings.description,
            roundTime: settings.roundTime,
            maxPlayerCount: settings.maxPlayerCount,
            minPlayerCount: settings.minPlayerCount,
            pointsToWin: settings.pointsToWin,
            isPrivateRoom: settings.isPrivateRoom,
            themesIds: settings.themes.filter(t => t.isSelected).map(t => t.id)
        };
    }

    renderModal() {
        const { name, words } = this.state.theme;
        const { wordToAdd } = this.state;

        return (
            <Modal onClose={this.closeModal}>
                <Modal.Header>Добавление темы</Modal.Header>
                <Modal.Body>
                    <div className={styles.themes__modal}>
                        <div className={styles.themes__modal_inputs}>
                            <label>
                                <div className='label'>Название</div>
                                <Input
                                    placeholder='Введите название'
                                    value={name}
                                    onChange={this.onChangeTheme('name')}/>
                            </label>
                            <label>
                                <div className='label'>Добавить слово</div>
                                <Gapped>
                                    <Input
                                        style = {{ ['margin-right'] : 10 }}
                                        placeholder='Введите слово'
                                        value={wordToAdd}
                                        onChange={(event) => this.setState({ wordToAdd: event.target.value })}
                                        onKeyPress={(event) => {
                                            if (event.key === 'Enter') {
                                                this.onNewWordEntered(wordToAdd);
                                            }
                                        }}
                                    />
                                    <Button onClick={() => this.onNewWordEntered(wordToAdd)}>Добавить</Button>
                                </Gapped>
                            </label>
                            <div className={styles.themes__words}>
                                {words.map(word => this.renderWord(word))}
                            </div>
                        </div>
                    </div>
                </Modal.Body>
                <Modal.Footer>
                    <Button use='success' onClick={() => this.saveTheme(this.state.theme)}>Сохранить</Button>
                </Modal.Footer>
            </Modal>
        );
    }

    renderWord(word) {
        return (
            <li onClick={() => this.onWordClicked(word)} className={styles.themes__words_word}>
                <a>{word}</a>
            </li>);
    }

    onNewWordEntered(word) {
        if (!this.state.theme.words.includes(word) && word.trim() !== '') {
            this.state.theme.words.push(word);
        }
        this.setState({ wordToAdd: '' });
    }

    onWordClicked(word) {
        this.setState({
            theme: {
                ...this.state.theme,
                words: this.state.theme.words.filter(w => w !== word)
            }
        });
    }

    onChangeTheme = (field) => {
        return (event) => {
            this.setState({
                theme: {
                    ...this.state.theme,
                    [field]: event.target.value,
                }
            });
        };
    };

    async saveTheme(theme) {
        let newThemeId = null;
        if (theme.name.trim() !== '' && theme.words.length > 0) {
            newThemeId = await authorizeFetch('/api/themes', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json;charset=utf-8'
                },
                body: JSON.stringify(theme)
            });
        }
        const themes = this.state.settings.themes;
        if (newThemeId !== null) {
            theme.id = newThemeId;
            themes.push(theme);
        }
        this.setState({
            modalOpened: false,
            theme: {
                name: '',
                words: []
            },
            settings: {
                themes: themes
            }
        });
    }


    openModal = () => {
        this.setState({
            modalOpened: true,
        });
    };

    closeModal = () => {
        this.setState({
            modalOpened: false,
        });
    };

    renderForm() {
        return <form
            className={styles.container}
        >
            <div className={styles.settings}>
                <h2 className={styles.header}>Настройки</h2>
                <div className="form-group">
                    <label>Название</label>
                    <input
                        className="form-control"
                        type="text"
                        value={this.state.settings.name}
                        onChange={(event) =>
                            this.handleSettingsChange('name', event.target.value)}
                        required={true}/>
                </div>
                <div className="form-group">
                    <label>Описание</label>
                    <input
                        className="form-control"
                        type="text"
                        value={this.state.settings.description}
                        onChange={(event) =>
                            this.handleSettingsChange('description', event.target.value)}
                        required={true}/>
                </div>
                <div className={`form-group ${styles.settings__item}`}>
                    <label>Количество игроков</label>
                    <select
                        value={this.state.settings.maxPlayerCount}
                        onChange={(event) =>
                            this.handleSettingsChange('maxPlayerCount', Number(event.target.value))}
                    >
                        <option selected value="5">5</option>
                        <option value="10">10</option>
                        <option value="15">15</option>
                        <option value="20">20</option>
                    </select>
                </div>
                <div className={`form-group ${styles.settings__item}`}>
                    <label>Минимальное количество игроков</label>
                    <select
                        value={this.state.settings.minPlayerCount}
                        onChange={(event) =>
                            this.handleSettingsChange('minPlayerCount', Number(event.target.value))}
                    >
                        <option selected value="2">2</option>
                        <option value="3">3</option>
                        <option value="5">5</option>
                        <option value="7">7</option>
                    </select>
                </div>
                <div className={`form-group ${styles.settings__item}`}>
                    <label>Количество очков для победы</label>
                    <select
                        value={this.state.settings.pointsToWin}
                        onChange={(event) =>
                            this.handleSettingsChange('pointsToWin', Number(event.target.value))}
                    >
                        <option selected value="100">100</option>
                        <option value="120">120</option>
                        <option value="150">150</option>
                        <option value="200">200</option>
                    </select>
                </div>
                <div className={`form-group ${styles.settings__item}`}>
                    <label>Время раунда</label>
                    <select
                        value={this.state.settings.roundTime}
                        onChange={(event) =>
                            this.handleSettingsChange('roundTime', Number(event.target.value))}
                    >
                        <option selected value="30">30 секунд</option>
                        <option value="60">60 секунд</option>
                        <option value="90">90 секунд</option>
                        <option value="120">120 секунд</option>
                    </select>
                </div>
                <div className={`form-group ${styles.settings__item}`}>
                    <label>Приватная</label>
                    <input
                        type="checkbox"
                        checked={this.state.settings.isPrivateRoom}
                        onChange={(event) =>
                            this.handleSettingsChange('isPrivateRoom', event.target.checked)}
                    />
                </div>
            </div>
            <div className={styles.themes}>
                <h2 className={styles.header}>Темы</h2>
                <ul className={styles.themes__list}>
                    {this.state.settings.themes.map(theme => this.renderTheme(theme))}
                    <li className={styles.themes__theme} onClick={this.openModal}>
                        {this.renderThemeSquare('+', false, styles.themes__add)}
                    </li>
                </ul>
            </div>
            <div className={styles.submit}>
                <button
                    onClick={this.handleSubmit}
                    className={`btn btn-success btn-lg ${styles.submit__button}`}
                >
                    Создать комнату
                </button>
            </div>
        </form>;
    }


    render() {
        return (
            this.state.loading
                ? <div className={styles.loading}>
                    <p>Загрузка...</p>
                </div>
                : <>{this.renderForm()}
                    {this.state.modalOpened && this.renderModal()}</>
        );
    }
}


export default withRouter(CreateRoom);