import React, {Component} from 'react';
import {Stage, Layer, Line} from 'react-konva';
import {RoomHubEvents} from "../RoomConstants";
import Palette from '../Palette/Palette';
import styles from './field.module.css';


export default class Field extends Component {
    constructor(props) {
        super(props);
        this.state = {
            lines: [],
            isLoadingField: true,
            isDrawing: false,
            stageRef: null,
            isActiveUser: false,
            activeUser: null,
            hiddenWord: null,
            drawingColor: '#000',
            isPalletShow: false
        };
    }

    async componentDidMount() {
        this.props.hubConnection.on(RoomHubEvents.NEW_LINE, (line) => {
            this.setState({
                lines: [...this.state.lines, line]
            });
        });

        this.props.hubConnection.on(RoomHubEvents.CLEAR_FIELD, () => {
            this.setState({
                lines: []
            });
        });

        this.props.hubConnection.on(RoomHubEvents.ROUND_INFO, (explainingPlayerName) => {
            const isActiveUser = explainingPlayerName === this.props.user.name;
            if (isActiveUser) {
                this.props.hubConnection.invoke(RoomHubEvents.REQUEST_WORD, this.props.roomId);
            }
            this.setState({isActiveUser});
            this.fetchLines();
        });

        this.props.hubConnection.on(RoomHubEvents.NEW_HIDDEN_WORD, (hiddenWord) => {
            this.setState({hiddenWord})
        });

        await this.fetchLines();
    }

    fetchLines = async () => {
        const response = await fetch(`/api/fields/${this.props.roomId}`);
        const lines = await response.json();
        this.setState({
            isLoadingField: false,
            lines: lines || []
        });
    }

    addLine = () => {
        this.props.hubConnection.invoke(RoomHubEvents.NEW_LINE,
            this.props.roomId, this.state.lines[this.state.lines.length - 1]);
    };
    
    getNewEmptyLine = () => {
        return {
            color: this.state.drawingColor,
            coordinates: []
        }
    }

    handleMouseDown = () => {
        this.state.isDrawing = true;
        this.setState({
            lines: [
                ...this.state.lines,
                this.getNewEmptyLine()
            ]
        });
    };

    handleMouseUp = () => {
        this.state.isDrawing = false;
        this.addLine();
    };

    handleMouseMove = () => {
        if (!this.state.isDrawing) {
            return;
        }
        const stage = this.state.stageRef.getStage();
        const point = stage.getPointerPosition();
        const {lines} = this.state;
        let lastLine = lines[lines.length - 1] || this.getNewEmptyLine();
        lastLine.coordinates = lastLine.coordinates.concat([point.x, point.y]) || [];
        lines.splice(lines.length - 1, 1, lastLine);
        this.setState({
            lines: lines.concat()
        });
    };

    clearField = () => {
        this.setState({lines: []});
        this.props.hubConnection.invoke(RoomHubEvents.CLEAR_FIELD, this.props.roomId);
    }

    changeDrawingColor = (drawingColor) => {
        this.setState({drawingColor})
    }

    onEraserClick = () => {
        this.setState({drawingColor: '#fff'})
    }

    render() {
        if (typeof window === 'undefined') {
            return null;
        }

        return (
            <section className={styles.field} id='field'>
                {this.state.isActiveUser ?
                    <>
                        <section className={styles.drawerTools}>
                            <Palette changeColor={this.changeDrawingColor}/>
                            <button className={styles.eraser}
                                    onClick={this.onEraserClick}/>
                            <button
                                className={`btn btn-dark btn-sm ${styles.clear}`}
                                onClick={this.clearField}
                            >
                                Очистить
                            </button>
                            <button
                                className={`btn btn-primary btn-sm ${styles.end}`}
                            >
                                Завершить
                            </button>
                        </section>
                        <div className={styles.wordContainer}>
                            <span
                                className={`btn btn-warning btn-sm ${styles.word}`}
                            >
                                {`Загаданное слово: ${this.state.hiddenWord}`}
                            </span>
                        </div>
                    </> : ''
                }
                {this.state.isLoadingField
                    ? <div className={styles.loading}>
                        <p>Загрузка...</p>
                    </div>
                    : <Stage
                        width={document.getElementById('field').offsetWidth - 3}
                        height=
                            {document.getElementById('field').offsetHeight -
                            (this.state.isActiveUser ? 65 : 0)}
                        onContentMousedown={this.state.isActiveUser ? this.handleMouseDown : () => {
                        }}
                        onContentMousemove={this.state.isActiveUser ? this.handleMouseMove : () => {
                        }}
                        onContentMouseup={this.state.isActiveUser ? this.handleMouseUp : () => {
                        }}
                        ref={node => {
                            this.state.stageRef = node
                        }}>
                        <Layer>
                            {this.state.lines.map((line, i) => (
                                <Line key={i} points={line.coordinates}
                                      stroke={line.color}
                                      strokeWidth={line.color === '#fff' ? 20 : 3}
                                />
                            ))}
                        </Layer>
                    </Stage>
                }
            </section>
        )
    }
}
